"""
מחרוזת החיבור למסד הנתונים, מתוך appsettings.json — לא קשיחה בקוד.

The C# side reads server, database, user and a DPAPI-encrypted password from
appsettings.json (Classes/DatabaseConfig.cs). The Python scripts used to carry
the sa password in clear text in their own source, which meant it shipped with
the application and sat in the repository. They now read the same file and
decrypt the same value, so there is one place to change credentials and no
password in any source file.

Decryption uses CryptUnprotectData through ctypes rather than pywin32, so no
package has to be installed: it is the same Windows call SecretProtector makes,
with the same entropy and the same LocalMachine scope, so a value produced by
MunicipalDiscount.SecretsSetup.exe is readable here and nowhere else.
"""

import ctypes
import ctypes.wintypes
import json
import os
import sys


# Must match Classes/Security/SecretProtector.cs exactly.
_ENTROPY = b"MunicipalDiscount-v1"
_CRYPTPROTECT_LOCAL_MACHINE = 0x4


class _DataBlob(ctypes.Structure):
    _fields_ = [("cbData", ctypes.wintypes.DWORD),
                ("pbData", ctypes.POINTER(ctypes.c_char))]


def _blob(data: bytes) -> _DataBlob:
    buffer = ctypes.create_string_buffer(data, len(data))
    return _DataBlob(len(data), ctypes.cast(buffer, ctypes.POINTER(ctypes.c_char)))


def _unprotect(encrypted: bytes, entropy: bytes) -> bytes:
    """CryptUnprotectData. Raises OSError with the Windows message on failure."""
    blob_in, blob_entropy, blob_out = _blob(encrypted), _blob(entropy), _DataBlob()

    ok = ctypes.windll.crypt32.CryptUnprotectData(
        ctypes.byref(blob_in), None, ctypes.byref(blob_entropy),
        None, None, _CRYPTPROTECT_LOCAL_MACHINE, ctypes.byref(blob_out))

    if not ok:
        raise ctypes.WinError(ctypes.get_last_error())

    try:
        return ctypes.string_at(blob_out.pbData, blob_out.cbData)
    finally:
        ctypes.windll.kernel32.LocalFree(blob_out.pbData)


def _settings_path() -> str:
    """
    appsettings.json sits beside the executable, and the scripts are deployed to
    PythonScripts\\ underneath it — so the file is one directory up. When a
    script is run by hand from the repository the same relative position holds
    against bin\\Debug\\net8.0-windows, which is checked second.
    """
    here = os.path.dirname(os.path.abspath(__file__))

    candidates = [
        os.path.join(os.path.dirname(here), "appsettings.json"),
        os.path.join(os.path.dirname(here), "bin", "Debug", "net8.0-windows", "appsettings.json"),
    ]

    for path in candidates:
        if os.path.isfile(path):
            return path

    raise FileNotFoundError(
        "appsettings.json not found. Looked in:\n  " + "\n  ".join(candidates))


def build_connection_string() -> str:
    """ODBC connection string, password decrypted at run time."""
    path = _settings_path()

    with open(path, encoding="utf-8-sig") as handle:
        settings = json.load(handle)["ConnectionString"]

    server = settings.get("Server", "")
    database = settings.get("Database", "")
    user = settings.get("User", "")
    encrypted = settings.get("EncryptedPassword", "")

    if not encrypted:
        raise ValueError(
            f"EncryptedPassword is empty in {path}. "
            "Run MunicipalDiscount.SecretsSetup.exe and paste the value it prints.")

    import base64
    try:
        password = _unprotect(base64.b64decode(encrypted), _ENTROPY).decode("utf-8")
    except OSError as error:
        raise RuntimeError(
            "Could not decrypt the database password on this machine. The value in "
            "appsettings.json was produced on a different computer — run "
            f"MunicipalDiscount.SecretsSetup.exe here and update it. ({error})") from error

    return (
        "Driver={ODBC Driver 17 for SQL Server};"
        f"Server={server};"
        f"Database={database};"
        f"UID={user};"
        f"PWD={password};"
        "TrustServerCertificate=yes;"
    )


# Built once on import, so a script that cannot reach the settings fails
# immediately with a clear message instead of at its first query.
try:
    CONN_STR = build_connection_string()
except Exception as _error:          # noqa: BLE001 - reported, not swallowed
    print(f"ERROR: {_error}")
    sys.exit(1)
