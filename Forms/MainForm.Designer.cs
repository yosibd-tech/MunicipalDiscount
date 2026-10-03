using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using System.Drawing;
using System.Windows.Forms;

namespace MunicipalDiscount
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            DevExpress.XtraEditors.Controls.EditorButtonImageOptions editorButtonImageOptions4 = new DevExpress.XtraEditors.Controls.EditorButtonImageOptions();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject13 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject14 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject15 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject16 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.XtraEditors.Controls.EditorButtonImageOptions editorButtonImageOptions3 = new DevExpress.XtraEditors.Controls.EditorButtonImageOptions();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject9 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject10 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject11 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject12 = new DevExpress.Utils.SerializableAppearanceObject();
            ReduceOperation reduceOperation2 = new ReduceOperation();
            ribbonMiniToolbar1 = new RibbonMiniToolbar(components);
            ribbonMiniToolbar2 = new RibbonMiniToolbar(components);
            barBtnSaveAs = new BarButtonItem();
            barButtonItem1 = new BarButtonItem();
            ribbonPage4 = new RibbonPage();
            ribbonPage5 = new RibbonPage();
            ribbonPageImportData = new RibbonPage();
            ribbonPageGroupImporCurrentYearData = new RibbonPageGroup();
            barEditImportPrevYear = new BarEditItem();
            repositoryItemTextEditYear = new RepositoryItemTextEdit();
            barButImportPreviouseYearData = new BarButtonItem();
            ribbonPageGroupImportPreviouseYearData = new RibbonPageGroup();
            barEditImportCurrYear = new BarEditItem();
            barButImportCurrentYearData = new BarButtonItem();
            ribbonPageGroupImportBilling = new RibbonPageGroup();
            barEditImportBillingYear = new BarEditItem();
            barButImportBilling = new BarButtonItem();
            ribbonPageSysTables = new RibbonPage();
            ribbonPageGroupSysTables = new RibbonPageGroup();
            barButSysTables = new BarButtonItem();
            ribbonPageGroupDiscountCodes = new RibbonPageGroup();
            barButDiscountCodes = new BarButtonItem();
            ribbonPageGroup3 = new RibbonPageGroup();
            barButCriterias = new BarButtonItem();
            ribbonPageGroupMinimumWage = new RibbonPageGroup();
            barButMinimumWage = new BarButtonItem();
            ribbonPageGroupParameters = new RibbonPageGroup();
            barButParameters = new BarButtonItem();
            ribbonPageGroupCopy = new RibbonPageGroup();
            barEditSourceYear = new BarEditItem();
            repositoryItemSpinEditSourceYear = new RepositoryItemSpinEdit();
            barEditTargetYe = new BarEditItem();
            repositoryItemSpinEditTargetYear = new RepositoryItemSpinEdit();
            barBtnCopyDiscounts = new BarButtonItem();
            ribbonPageGroupUserPermissions = new RibbonPageGroup();
            barButUserPermissions = new BarButtonItem();
            barButAdmin = new BarButtonItem();
            ribbonPagePrint = new RibbonPage();
            ribbonPageGroupPrint = new RibbonPageGroup();
            barBtnPrint = new BarButtonItem();
            ribbonPageReports = new RibbonPage();
            ribbonPageGroupReports = new RibbonPageGroup();
            barSubItem2 = new BarSubItem();
            barButMultiDiscounts = new BarButtonItem();
            barButReportPreviouseYear = new BarButtonItem();
            barButReportCurrentYear = new BarButtonItem();
            barStaticReport3Title = new BarStaticItem();
            barEditReport3Year = new BarEditItem();
            repositoryItemTextEditReport3Year = new RepositoryItemTextEdit();
            barBtnRunReport3 = new BarEditItem();
            repositoryItemBtnRunReport3 = new RepositoryItemButtonEdit();
            barBtnSendSMS = new BarButtonItem();
            barStaticReportTitle = new BarStaticItem();
            barEditReportYear = new BarEditItem();
            repositoryItemTextEditReportYear = new RepositoryItemTextEdit();
            barBtnRunReport = new BarEditItem();
            repositoryItemBtnRunReport = new RepositoryItemButtonEdit();
            barSubItem1 = new BarSubItem();
            barStaticReport2Title = new BarStaticItem();
            barEditReport2Year = new BarEditItem();
            repositoryItemTextEditReport2Year = new RepositoryItemTextEdit();
            barBtnRunReport2 = new BarEditItem();
            repositoryItemBtnRunReport2 = new RepositoryItemButtonEdit();
            barCheckItemPreviouseYear = new BarCheckItem();
            barCheckItemCurrentYear = new BarCheckItem();
            ribbonPage2 = new RibbonPage();
            ribbonPageGroup2 = new RibbonPageGroup();
            barEditYear = new BarEditItem();
            ribbonPageGroup1 = new RibbonPageGroup();
            barBtnDiscountEligibility = new BarButtonItem();
            ribbonPage1 = new RibbonPage();
            ribbonPageGroupWorkers = new RibbonPageGroup();
            barStaticUserName = new BarStaticItem();
            ribbonPageGroupSystem = new RibbonPageGroup();
            barButConnection = new BarButtonItem();
            ribbonGalleryBarItem1 = new RibbonGalleryBarItem();
            barSubItemYear = new BarSubItem();
            ribbonControl1 = new RibbonControl();
            barButtonItem2 = new BarButtonItem();
            barButImportRequestDates = new BarButtonItem();
            ribbonPage3 = new RibbonPage();
            ribbonPageGroup4 = new RibbonPageGroup();
            ribbonPageGroup5 = new RibbonPageGroup();
            ribbonPage7 = new RibbonPage();
            ribbonPageGroup6 = new RibbonPageGroup();
            barButMailSetup = new BarButtonItem();
            ((System.ComponentModel.ISupportInitialize)repositoryItemTextEditYear).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemSpinEditSourceYear).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemSpinEditTargetYear).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemTextEditReport3Year).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemBtnRunReport3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemTextEditReportYear).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemBtnRunReport).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemTextEditReport2Year).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemBtnRunReport2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ribbonControl1).BeginInit();
            SuspendLayout();
            // 
            // ribbonMiniToolbar1
            // 
            ribbonMiniToolbar1.ParentControl = this;
            // 
            // ribbonMiniToolbar2
            // 
            ribbonMiniToolbar2.ParentControl = this;
            // 
            // barBtnSaveAs
            // 
            barBtnSaveAs.Caption = "שמור כ...";
            barBtnSaveAs.Id = 2;
            barBtnSaveAs.ImageOptions.Image = (Image)resources.GetObject("barBtnSaveAs.ImageOptions.Image");
            barBtnSaveAs.ImageOptions.LargeImage = (Image)resources.GetObject("barBtnSaveAs.ImageOptions.LargeImage");
            barBtnSaveAs.LargeWidth = 75;
            barBtnSaveAs.Name = "barBtnSaveAs";
            barBtnSaveAs.RibbonStyle = RibbonItemStyles.Large;
            barBtnSaveAs.ItemClick += barBtnSaveAs_ItemClick;
            // 
            // barButtonItem1
            // 
            barButtonItem1.Caption = "בחר רשות";
            barButtonItem1.Id = 8;
            barButtonItem1.Name = "barButtonItem1";
            // 
            // ribbonPage4
            // 
            ribbonPage4.Name = "ribbonPage4";
            ribbonPage4.Text = "ribbonPage4";
            // 
            // ribbonPage5
            // 
            ribbonPage5.Name = "ribbonPage5";
            ribbonPage5.Text = "ribbonPage5";
            // 
            // ribbonPageImportData
            // 
            ribbonPageImportData.Groups.AddRange(new RibbonPageGroup[] { ribbonPageGroupImporCurrentYearData, ribbonPageGroupImportPreviouseYearData, ribbonPageGroupImportBilling });
            ribbonPageImportData.Name = "ribbonPageImportData";
            ribbonPageImportData.Text = "יבוא נתונים למערכת";
            // 
            // ribbonPageGroupImporCurrentYearData
            // 
            ribbonPageGroupImporCurrentYearData.ItemLinks.Add(barEditImportPrevYear);
            ribbonPageGroupImporCurrentYearData.ItemLinks.Add(barButImportPreviouseYearData);
            ribbonPageGroupImporCurrentYearData.Name = "ribbonPageGroupImporCurrentYearData";
            // 
            // barEditImportPrevYear
            // 
            barEditImportPrevYear.Caption = "שנה:";
            barEditImportPrevYear.Edit = repositoryItemTextEditYear;
            barEditImportPrevYear.EditWidth = 60;
            barEditImportPrevYear.Id = 201;
            barEditImportPrevYear.Name = "barEditImportPrevYear";
            // 
            // repositoryItemTextEditYear
            // 
            repositoryItemTextEditYear.AutoHeight = false;
            repositoryItemTextEditYear.Mask.EditMask = "0000";
            repositoryItemTextEditYear.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            repositoryItemTextEditYear.Mask.UseMaskAsDisplayFormat = true;
            repositoryItemTextEditYear.Name = "repositoryItemTextEditYear";
            // 
            // barButImportPreviouseYearData
            // 
            barButImportPreviouseYearData.Caption = "יבוא נתוני ש.קודמת";
            barButImportPreviouseYearData.Id = 52;
            barButImportPreviouseYearData.ImageOptions.Image = (Image)resources.GetObject("barButImportPreviouseYearData.ImageOptions.Image");
            barButImportPreviouseYearData.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("barButImportPreviouseYearData.ImageOptions.SvgImage");
            barButImportPreviouseYearData.Name = "barButImportPreviouseYearData";
            barButImportPreviouseYearData.ItemClick += barButImportPreviouseYearData_ItemClick;
            // 
            // ribbonPageGroupImportPreviouseYearData
            // 
            ribbonPageGroupImportPreviouseYearData.ItemLinks.Add(barEditImportCurrYear);
            ribbonPageGroupImportPreviouseYearData.ItemLinks.Add(barButImportCurrentYearData);
            ribbonPageGroupImportPreviouseYearData.Name = "ribbonPageGroupImportPreviouseYearData";
            // 
            // barEditImportCurrYear
            // 
            barEditImportCurrYear.Caption = "שנה:";
            barEditImportCurrYear.Edit = repositoryItemTextEditYear;
            barEditImportCurrYear.EditWidth = 60;
            barEditImportCurrYear.Id = 202;
            barEditImportCurrYear.Name = "barEditImportCurrYear";
            // 
            // barButImportCurrentYearData
            // 
            barButImportCurrentYearData.Caption = "יבוא נתוני שנה שוטפת";
            barButImportCurrentYearData.Id = 53;
            barButImportCurrentYearData.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("barButImportCurrentYearData.ImageOptions.SvgImage");
            barButImportCurrentYearData.Name = "barButImportCurrentYearData";
            barButImportCurrentYearData.ItemClick += barButImportCurrentYearData_ItemClick;
            // 
            // ribbonPageGroupImportBilling
            // 
            ribbonPageGroupImportBilling.ItemLinks.Add(barEditImportBillingYear);
            ribbonPageGroupImportBilling.ItemLinks.Add(barButImportBilling);
            ribbonPageGroupImportBilling.Name = "ribbonPageGroupImportBilling";
            // 
            // barEditImportBillingYear
            // 
            barEditImportBillingYear.Caption = "שנה:";
            barEditImportBillingYear.Edit = repositoryItemTextEditYear;
            barEditImportBillingYear.EditWidth = 60;
            barEditImportBillingYear.Id = 203;
            barEditImportBillingYear.Name = "barEditImportBillingYear";
            // 
            // barButImportBilling
            // 
            barButImportBilling.Caption = "יבוא חיוב שנתי/תקופתי";
            barButImportBilling.Id = 71;
            barButImportBilling.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("barButImportBilling.ImageOptions.SvgImage");
            barButImportBilling.Name = "barButImportBilling";
            barButImportBilling.ItemClick += barButImportBilling_ItemClick;
            // 
            // ribbonPageSysTables
            // 
            ribbonPageSysTables.Groups.AddRange(new RibbonPageGroup[] { ribbonPageGroupSysTables, ribbonPageGroupDiscountCodes, ribbonPageGroup3, ribbonPageGroupMinimumWage, ribbonPageGroupParameters, ribbonPageGroupCopy, ribbonPageGroupUserPermissions, ribbonPageGroup6 });
            ribbonPageSysTables.Name = "ribbonPageSysTables";
            ribbonPageSysTables.Text = "טבלאות המערכת";
            // 
            // ribbonPageGroupSysTables
            // 
            ribbonPageGroupSysTables.ItemLinks.Add(barButSysTables);
            ribbonPageGroupSysTables.Name = "ribbonPageGroupSysTables";
            // 
            // barButSysTables
            // 
            barButSysTables.Caption = "כותרות קובץ הנחות";
            barButSysTables.Id = 47;
            barButSysTables.Name = "barButSysTables";
            barButSysTables.ItemClick += barButSysTables_ItemClick;
            // 
            // ribbonPageGroupDiscountCodes
            // 
            ribbonPageGroupDiscountCodes.ItemLinks.Add(barButDiscountCodes);
            ribbonPageGroupDiscountCodes.Name = "ribbonPageGroupDiscountCodes";
            // 
            // barButDiscountCodes
            // 
            barButDiscountCodes.Caption = "קודי הנחות";
            barButDiscountCodes.Id = 48;
            barButDiscountCodes.Name = "barButDiscountCodes";
            barButDiscountCodes.ItemClick += barButDiscountCodes_ItemClick;
            // 
            // ribbonPageGroup3
            // 
            ribbonPageGroup3.ItemLinks.Add(barButCriterias);
            ribbonPageGroup3.Name = "ribbonPageGroup3";
            // 
            // barButCriterias
            // 
            barButCriterias.Caption = "קרטריונים למבחן הכנסה";
            barButCriterias.Id = 49;
            barButCriterias.Name = "barButCriterias";
            barButCriterias.ItemClick += barButCriterias_ItemClick;
            // 
            // ribbonPageGroupMinimumWage
            // 
            ribbonPageGroupMinimumWage.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("ribbonPageGroupMinimumWage.ImageOptions.SvgImage");
            ribbonPageGroupMinimumWage.ItemLinks.Add(barButMinimumWage);
            ribbonPageGroupMinimumWage.Name = "ribbonPageGroupMinimumWage";
            // 
            // barButMinimumWage
            // 
            barButMinimumWage.Caption = "עדכון שכר מינימום";
            barButMinimumWage.Id = 50;
            barButMinimumWage.Name = "barButMinimumWage";
            barButMinimumWage.ItemClick += barButMinimumWage_ItemClick;
            // 
            // ribbonPageGroupParameters
            // 
            ribbonPageGroupParameters.ItemLinks.Add(barButParameters);
            ribbonPageGroupParameters.Name = "ribbonPageGroupParameters";
            // 
            // barButParameters
            // 
            barButParameters.Caption = "עידכון\r\n פרמטרים";
            barButParameters.CategoryGuid = new Guid("75e54f03-3bb3-4ec3-abff-895e369e5eed");
            barButParameters.Id = 51;
            barButParameters.Name = "barButParameters";
            barButParameters.ItemClick += barButParameters_ItemClick;
            // 
            // ribbonPageGroupCopy
            // 
            ribbonPageGroupCopy.ItemLinks.Add(barEditSourceYear);
            ribbonPageGroupCopy.ItemLinks.Add(barEditTargetYe);
            ribbonPageGroupCopy.ItemLinks.Add(barBtnCopyDiscounts);
            ribbonPageGroupCopy.Name = "ribbonPageGroupCopy";
            ribbonPageGroupCopy.Text = "העתקת קודי הנחה";
            // 
            // barEditSourceYear
            // 
            barEditSourceYear.Caption = "משנה:";
            barEditSourceYear.Edit = repositoryItemSpinEditSourceYear;
            barEditSourceYear.EditWidth = 60;
            barEditSourceYear.Id = 28;
            barEditSourceYear.Name = "barEditSourceYear";
            // 
            // repositoryItemSpinEditSourceYear
            // 
            repositoryItemSpinEditSourceYear.AutoHeight = false;
            repositoryItemSpinEditSourceYear.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            repositoryItemSpinEditSourceYear.IsFloatValue = false;
            repositoryItemSpinEditSourceYear.MaxValue = new decimal(new int[] { 2100, 0, 0, 0 });
            repositoryItemSpinEditSourceYear.MinValue = new decimal(new int[] { 2000, 0, 0, 0 });
            repositoryItemSpinEditSourceYear.Name = "repositoryItemSpinEditSourceYear";
            // 
            // barEditTargetYe
            // 
            barEditTargetYe.Caption = "בחר שנה";
            barEditTargetYe.Edit = repositoryItemSpinEditTargetYear;
            barEditTargetYe.EditWidth = 60;
            barEditTargetYe.Id = 29;
            barEditTargetYe.Name = "barEditTargetYe";
            // 
            // repositoryItemSpinEditTargetYear
            // 
            repositoryItemSpinEditTargetYear.AutoHeight = false;
            repositoryItemSpinEditTargetYear.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            repositoryItemSpinEditTargetYear.IsFloatValue = false;
            repositoryItemSpinEditTargetYear.MaxValue = new decimal(new int[] { 2100, 0, 0, 0 });
            repositoryItemSpinEditTargetYear.MinValue = new decimal(new int[] { 2000, 0, 0, 0 });
            repositoryItemSpinEditTargetYear.Name = "repositoryItemSpinEditTargetYear";
            // 
            // barBtnCopyDiscounts
            // 
            barBtnCopyDiscounts.Caption = "לחץ להעתקה";
            barBtnCopyDiscounts.Id = 30;
            barBtnCopyDiscounts.ItemAppearance.Normal.ForeColor = Color.Chocolate;
            barBtnCopyDiscounts.ItemAppearance.Normal.Options.UseForeColor = true;
            barBtnCopyDiscounts.ItemInMenuAppearance.Normal.Font = new Font("Tahoma", 7.8F, FontStyle.Bold);
            barBtnCopyDiscounts.ItemInMenuAppearance.Normal.Options.UseFont = true;
            barBtnCopyDiscounts.Name = "barBtnCopyDiscounts";
            barBtnCopyDiscounts.ItemClick += barBtnCopyDiscounts_ItemClick;
            // 
            // ribbonPageGroupUserPermissions
            // 
            ribbonPageGroupUserPermissions.ItemLinks.Add(barButUserPermissions);
            ribbonPageGroupUserPermissions.Name = "ribbonPageGroupUserPermissions";
            // 
            // barButUserPermissions
            // 
            barButUserPermissions.Caption = "ניהול משתמשים והרשאות";
            barButUserPermissions.Id = 77;
            barButUserPermissions.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("barButUserPermissions.ImageOptions.SvgImage");
            barButUserPermissions.Name = "barButUserPermissions";
            barButUserPermissions.ItemClick += barButUserPermissions_ItemClick;
            // 
            // barButAdmin
            // 
            barButAdmin.Caption = "ניהול מערכת";
            barButAdmin.CategoryGuid = new Guid("75e54f03-3bb3-4ec3-abff-895e369e5eed");
            barButAdmin.Id = 96;
            barButAdmin.Name = "barButAdmin";
            barButAdmin.ItemClick += barButAdmin_ItemClick;
            // 
            // ribbonPagePrint
            // 
            ribbonPagePrint.Groups.AddRange(new RibbonPageGroup[] { ribbonPageGroupPrint });
            ribbonPagePrint.Name = "ribbonPagePrint";
            ribbonPagePrint.Text = "הדפסה";
            // 
            // ribbonPageGroupPrint
            // 
            ribbonPageGroupPrint.AllowTextClipping = false;
            ribbonPageGroupPrint.ItemLinks.Add(barBtnPrint);
            ribbonPageGroupPrint.Name = "ribbonPageGroupPrint";
            ribbonPageGroupPrint.Text = "מיועד להדפסת טפסים";
            // 
            // barBtnPrint
            // 
            barBtnPrint.Caption = "הדפסה";
            barBtnPrint.Id = 1;
            barBtnPrint.ImageOptions.Image = (Image)resources.GetObject("barBtnPrint.ImageOptions.Image");
            barBtnPrint.ImageOptions.LargeImage = (Image)resources.GetObject("barBtnPrint.ImageOptions.LargeImage");
            barBtnPrint.Name = "barBtnPrint";
            barBtnPrint.RibbonStyle = RibbonItemStyles.Large;
            barBtnPrint.ItemClick += barBtnPrint_ItemClick;
            // 
            // ribbonPageReports
            // 
            ribbonPageReports.Groups.AddRange(new RibbonPageGroup[] { ribbonPageGroupReports });
            ribbonPageReports.Name = "ribbonPageReports";
            ribbonPageReports.Text = "דוחות";
            // 
            // ribbonPageGroupReports
            // 
            ribbonPageGroupReports.AllowTextClipping = false;
            ribbonPageGroupReports.ItemLinks.Add(barSubItem2);
            ribbonPageGroupReports.Name = "ribbonPageGroupReports";
            ribbonPageGroupReports.Text = "דוחות ניתוח הנחות ";
            // 
            // barSubItem2
            // 
            barSubItem2.Caption = "בחר דוח";
            barSubItem2.Id = 64;
            barSubItem2.LinksPersistInfo.AddRange(new LinkPersistInfo[] { new LinkPersistInfo(barButReportPreviouseYear), new LinkPersistInfo(barButReportCurrentYear), new LinkPersistInfo(barButMultiDiscounts) });
            barSubItem2.Name = "barSubItem2";
            // 
            // barButReportPreviouseYear
            // 
            barButMultiDiscounts.Caption = "כפל הנחות";
            barButMultiDiscounts.Id = 97;
            barButMultiDiscounts.Name = "barButMultiDiscounts";
            //
            // barButReportPreviouseYear
            //
            barButReportPreviouseYear.Caption = "סיכום הנחות שניתנו";
            barButReportPreviouseYear.Id = 65;
            barButReportPreviouseYear.Name = "barButReportPreviouseYear";
            barButReportPreviouseYear.ItemClick += barButReportPreviouseYear_ItemClick;
            // 
            // barButReportCurrentYear
            // 
            barButReportCurrentYear.Caption = "ניתוח הנחות מול המגיע";
            barButReportCurrentYear.Id = 69;
            barButReportCurrentYear.Name = "barButReportCurrentYear";
            barButReportCurrentYear.ItemClick += barButReportCurrentYear_ItemClick;
            // 
            // barStaticReport3Title
            // 
            barStaticReport3Title.AllowHtmlText = DevExpress.Utils.DefaultBoolean.True;
            barStaticReport3Title.Id = 41;
            barStaticReport3Title.ItemAppearance.Normal.Font = new Font("Tahoma", 7.8F, FontStyle.Regular, GraphicsUnit.Point, 177);
            barStaticReport3Title.ItemAppearance.Normal.Options.UseFont = true;
            barStaticReport3Title.Name = "barStaticReport3Title";
            // 
            // barEditReport3Year
            // 
            barEditReport3Year.Caption = "שנה";
            barEditReport3Year.Edit = repositoryItemTextEditReport3Year;
            barEditReport3Year.EditWidth = 80;
            barEditReport3Year.Id = 42;
            barEditReport3Year.Name = "barEditReport3Year";
            // 
            // repositoryItemTextEditReport3Year
            // 
            repositoryItemTextEditReport3Year.AutoHeight = false;
            repositoryItemTextEditReport3Year.Mask.EditMask = "0000";
            repositoryItemTextEditReport3Year.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            repositoryItemTextEditReport3Year.Mask.UseMaskAsDisplayFormat = true;
            repositoryItemTextEditReport3Year.Name = "repositoryItemTextEditReport3Year";
            // 
            // barBtnRunReport3
            // 
            barBtnRunReport3.Caption = "שנה";
            barBtnRunReport3.Edit = repositoryItemBtnRunReport3;
            barBtnRunReport3.EditWidth = 80;
            barBtnRunReport3.Id = 43;
            barBtnRunReport3.ImageOptions.Image = (Image)resources.GetObject("barBtnRunReport3.ImageOptions.Image");
            barBtnRunReport3.ItemAppearance.Normal.ForeColor = Color.Transparent;
            barBtnRunReport3.ItemAppearance.Normal.Options.UseForeColor = true;
            barBtnRunReport3.Name = "barBtnRunReport3";
            // 
            // repositoryItemBtnRunReport3
            // 
            repositoryItemBtnRunReport3.AutoHeight = false;
            repositoryItemBtnRunReport3.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(), new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph, "לחץ", -1, true, true, false, editorButtonImageOptions4, new DevExpress.Utils.KeyShortcut(Keys.None), serializableAppearanceObject13, serializableAppearanceObject14, serializableAppearanceObject15, serializableAppearanceObject16, "", null, null, DevExpress.Utils.ToolTipAnchor.Default) });
            repositoryItemBtnRunReport3.Name = "repositoryItemBtnRunReport3";
            repositoryItemBtnRunReport3.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.HideTextEditor;
            repositoryItemBtnRunReport3.ButtonClick += barBtnRunReport3_ButtonClick;
            // 
            // barBtnSendSMS
            // 
            barBtnSendSMS.Caption = "שלח SMS";
            barBtnSendSMS.Id = 44;
            barBtnSendSMS.Name = "barBtnSendSMS";
            barBtnSendSMS.ItemClick += barBtnSendSMS_ItemClick;
            // 
            // barStaticReportTitle
            // 
            barStaticReportTitle.Id = 37;
            barStaticReportTitle.ItemAppearance.Normal.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 177);
            barStaticReportTitle.ItemAppearance.Normal.Options.UseFont = true;
            barStaticReportTitle.Name = "barStaticReportTitle";
            // 
            // barEditReportYear
            // 
            barEditReportYear.Alignment = BarItemLinkAlignment.Left;
            barEditReportYear.Caption = "שנה";
            barEditReportYear.Edit = repositoryItemTextEditReportYear;
            barEditReportYear.EditWidth = 80;
            barEditReportYear.Id = 33;
            barEditReportYear.Name = "barEditReportYear";
            // 
            // repositoryItemTextEditReportYear
            // 
            repositoryItemTextEditReportYear.AutoHeight = false;
            repositoryItemTextEditReportYear.Mask.EditMask = "0000";
            repositoryItemTextEditReportYear.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            repositoryItemTextEditReportYear.Mask.UseMaskAsDisplayFormat = true;
            repositoryItemTextEditReportYear.Name = "repositoryItemTextEditReportYear";
            // 
            // barBtnRunReport
            // 
            barBtnRunReport.Alignment = BarItemLinkAlignment.Right;
            barBtnRunReport.Caption = "שנה";
            barBtnRunReport.Edit = repositoryItemBtnRunReport;
            barBtnRunReport.EditWidth = 80;
            barBtnRunReport.Id = 34;
            barBtnRunReport.ImageOptions.Image = (Image)resources.GetObject("barBtnRunReport.ImageOptions.Image");
            barBtnRunReport.ItemAppearance.Normal.ForeColor = Color.Transparent;
            barBtnRunReport.ItemAppearance.Normal.Options.UseForeColor = true;
            barBtnRunReport.Name = "barBtnRunReport";
            // 
            // repositoryItemBtnRunReport
            // 
            repositoryItemBtnRunReport.AutoHeight = false;
            editorButtonImageOptions3.EnableTransparency = false;
            repositoryItemBtnRunReport.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph, "לחץ", -1, true, true, false, editorButtonImageOptions3, new DevExpress.Utils.KeyShortcut(Keys.None), serializableAppearanceObject9, serializableAppearanceObject10, serializableAppearanceObject11, serializableAppearanceObject12, "", null, null, DevExpress.Utils.ToolTipAnchor.Default) });
            repositoryItemBtnRunReport.ButtonsStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            repositoryItemBtnRunReport.Name = "repositoryItemBtnRunReport";
            repositoryItemBtnRunReport.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.HideTextEditor;
            repositoryItemBtnRunReport.ButtonClick += barBtnRunReport_ButtonClick;
            // 
            // barSubItem1
            // 
            barSubItem1.Caption = "barSubItem1";
            barSubItem1.Id = 66;
            barSubItem1.Name = "barSubItem1";
            // 
            // barStaticReport2Title
            // 
            barStaticReport2Title.Id = 38;
            barStaticReport2Title.ItemAppearance.Normal.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 177);
            barStaticReport2Title.ItemAppearance.Normal.Options.UseFont = true;
            barStaticReport2Title.Name = "barStaticReport2Title";
            // 
            // barEditReport2Year
            // 
            barEditReport2Year.Caption = "שנה";
            barEditReport2Year.Edit = repositoryItemTextEditReport2Year;
            barEditReport2Year.EditWidth = 80;
            barEditReport2Year.Id = 39;
            barEditReport2Year.Name = "barEditReport2Year";
            // 
            // repositoryItemTextEditReport2Year
            // 
            repositoryItemTextEditReport2Year.AutoHeight = false;
            repositoryItemTextEditReport2Year.Mask.EditMask = "0000";
            repositoryItemTextEditReport2Year.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            repositoryItemTextEditReport2Year.Mask.UseMaskAsDisplayFormat = true;
            repositoryItemTextEditReport2Year.Name = "repositoryItemTextEditReport2Year";
            // 
            // barBtnRunReport2
            // 
            barBtnRunReport2.Edit = null;
            barBtnRunReport2.Id = 68;
            barBtnRunReport2.Name = "barBtnRunReport2";
            // 
            // repositoryItemBtnRunReport2
            // 
            repositoryItemBtnRunReport2.Name = "repositoryItemBtnRunReport2";
            // 
            // barCheckItemPreviouseYear
            // 
            barCheckItemPreviouseYear.Caption = "2025";
            barCheckItemPreviouseYear.GroupIndex = 1;
            barCheckItemPreviouseYear.Id = 61;
            barCheckItemPreviouseYear.Name = "barCheckItemPreviouseYear";
            // 
            // barCheckItemCurrentYear
            // 
            barCheckItemCurrentYear.BindableChecked = true;
            barCheckItemCurrentYear.Caption = "2026";
            barCheckItemCurrentYear.Checked = true;
            barCheckItemCurrentYear.Id = 63;
            barCheckItemCurrentYear.Name = "barCheckItemCurrentYear";
            // 
            // ribbonPage2
            // 
            ribbonPage2.Groups.AddRange(new RibbonPageGroup[] { ribbonPageGroup2, ribbonPageGroup1 });
            ribbonPage2.Name = "ribbonPage2";
            ribbonPage2.Text = "מבחן הכנסה";
            // 
            // ribbonPageGroup2
            // 
            ribbonPageGroup2.ItemLinks.Add(barEditYear);
            ribbonPageGroup2.Name = "ribbonPageGroup2";
            // 
            // barEditYear
            // 
            barEditYear.Caption = "שנה:               ";
            barEditYear.Edit = repositoryItemTextEditYear;
            barEditYear.EditWidth = 80;
            barEditYear.Id = 5;
            barEditYear.Name = "barEditYear";
            barEditYear.EditValueChanged += barEditYear_EditValueChanged;
            // 
            // ribbonPageGroup1
            // 
            ribbonPageGroup1.ItemLinks.Add(barBtnDiscountEligibility);
            ribbonPageGroup1.Name = "ribbonPageGroup1";
            // 
            // barBtnDiscountEligibility
            // 
            barBtnDiscountEligibility.ButtonStyle = BarButtonStyle.DropDown;
            barBtnDiscountEligibility.Caption = "בדיקת זכאות להנחה";
            barBtnDiscountEligibility.Id = 27;
            barBtnDiscountEligibility.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("barBtnDiscountEligibility.ImageOptions.SvgImage");
            barBtnDiscountEligibility.ItemAppearance.Normal.Options.UseFont = true;
            barBtnDiscountEligibility.Name = "barBtnDiscountEligibility";
            barBtnDiscountEligibility.ItemClick += barBtnDiscountEligibility_ItemClick;
            // 
            // ribbonPage1
            // 
            ribbonPage1.Groups.AddRange(new RibbonPageGroup[] { ribbonPageGroupWorkers, ribbonPageGroupSystem });
            ribbonPage1.Name = "ribbonPage1";
            reduceOperation2.Behavior = ReduceOperationBehavior.Single;
            reduceOperation2.GroupName = null;
            reduceOperation2.ItemLinkIndex = 0;
            reduceOperation2.ItemLinksCount = 0;
            reduceOperation2.Operation = ReduceOperationType.ButtonGroups;
            ribbonPage1.ReduceOperations.Add(reduceOperation2);
            ribbonPage1.Text = "ראשי";
            // 
            // ribbonPageGroupWorkers
            // 
            ribbonPageGroupWorkers.ItemLinks.Add(barStaticUserName);
            ribbonPageGroupWorkers.Name = "ribbonPageGroupWorkers";
            // 
            // barStaticUserName
            // 
            barStaticUserName.Caption = "יציאה מהמערכת";
            barStaticUserName.Id = 3;
            barStaticUserName.ItemAppearance.Normal.Font = new Font("Tahoma", 7.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            barStaticUserName.ItemAppearance.Normal.Options.UseFont = true;
            barStaticUserName.Name = "barStaticUserName";
            // 
            // ribbonPageGroupSystem
            // 
            ribbonPageGroupSystem.ItemLinks.Add(barButAdmin);
            ribbonPageGroupSystem.ItemLinks.Add(barButConnection);
            ribbonPageGroupSystem.Name = "ribbonPageGroupSystem";
            ribbonPageGroupSystem.Text = "מערכת";
            // 
            // barButConnection
            // 
            barButConnection.Caption = "הגדרות חיבור";
            barButConnection.CategoryGuid = new Guid("75e54f03-3bb3-4ec3-abff-895e369e5eed");
            barButConnection.Id = 97;
            barButConnection.Name = "barButConnection";
            barButConnection.ItemClick += barButConnection_ItemClick;
            // 
            // ribbonGalleryBarItem1
            // 
            ribbonGalleryBarItem1.Caption = "ribbonGalleryBarItem1";
            ribbonGalleryBarItem1.Id = 41;
            ribbonGalleryBarItem1.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("ribbonGalleryBarItem1.ImageOptions.SvgImage");
            ribbonGalleryBarItem1.Name = "ribbonGalleryBarItem1";
            // 
            // barSubItemYear
            // 
            barSubItemYear.Caption = "בחר שנה";
            barSubItemYear.Id = 56;
            barSubItemYear.Name = "barSubItemYear";
            // 
            // ribbonControl1
            // 
            ribbonControl1.ExpandCollapseItem.Id = 0;
            ribbonControl1.Items.AddRange(new BarItem[] { ribbonControl1.ExpandCollapseItem, barBtnPrint, barStaticUserName, barEditYear, barBtnDiscountEligibility, barEditSourceYear, barEditTargetYe, barBtnCopyDiscounts, barStaticReportTitle, barEditReportYear, barBtnRunReport, barStaticReport2Title, barEditReport2Year, barBtnRunReport2, barStaticReport3Title, barEditReport3Year, barBtnRunReport3, barBtnSendSMS, ribbonGalleryBarItem1, barButSysTables, barButDiscountCodes, barButCriterias, barButMinimumWage, barButParameters, barButAdmin, barButConnection, barButImportPreviouseYearData, barButImportCurrentYearData, barSubItemYear, barCheckItemPreviouseYear, barCheckItemCurrentYear, barSubItem2, barButReportPreviouseYear, barSubItem1, barButReportCurrentYear, barButImportBilling, barButtonItem2, barButImportRequestDates, barButUserPermissions, barEditImportPrevYear, barEditImportCurrYear, barEditImportBillingYear, barButMailSetup, barButMultiDiscounts });
            ribbonControl1.Location = new Point(0, 0);
            ribbonControl1.MaxItemId = 79;
            ribbonControl1.MiniToolbars.Add(ribbonMiniToolbar1);
            ribbonControl1.MiniToolbars.Add(ribbonMiniToolbar2);
            ribbonControl1.Name = "ribbonControl1";
            ribbonControl1.Pages.AddRange(new RibbonPage[] { ribbonPage1, ribbonPageImportData, ribbonPage3, ribbonPage2, ribbonPageSysTables, ribbonPagePrint, ribbonPageReports });
            ribbonControl1.RepositoryItems.AddRange(new RepositoryItem[] { repositoryItemTextEditYear, repositoryItemSpinEditSourceYear, repositoryItemSpinEditTargetYear, repositoryItemTextEditReportYear, repositoryItemBtnRunReport, repositoryItemTextEditReport2Year, repositoryItemBtnRunReport2, repositoryItemTextEditReport3Year, repositoryItemBtnRunReport3 });
            ribbonControl1.RightToLeft = RightToLeft.Yes;
            ribbonControl1.Size = new Size(1200, 193);
            // 
            // barButtonItem2
            // 
            barButtonItem2.Caption = "פתח טופס";
            barButtonItem2.Id = 76;
            barButtonItem2.Name = "barButtonItem2";
            // 
            // barButImportRequestDates
            // 
            barButImportRequestDates.Caption = "קליטת תאריכים מקובץ תנועות";
            barButImportRequestDates.Id = 78;
            barButImportRequestDates.Name = "barButImportRequestDates";
            // 
            // ribbonPage3
            // 
            ribbonPage3.Groups.AddRange(new RibbonPageGroup[] { ribbonPageGroup4, ribbonPageGroup5 });
            ribbonPage3.Name = "ribbonPage3";
            ribbonPage3.Text = "עידכון תאריך בקשת הנחה";
            // 
            // ribbonPageGroup4
            // 
            ribbonPageGroup4.ItemLinks.Add(barEditTargetYe);
            ribbonPageGroup4.Name = "ribbonPageGroup4";
            ribbonPageGroup4.Text = "ribbonPageGroup4";
            // 
            // ribbonPageGroup5
            // 
            ribbonPageGroup5.ItemLinks.Add(barButtonItem2);
            ribbonPageGroup5.ItemLinks.Add(barButImportRequestDates);
            ribbonPageGroup5.Name = "ribbonPageGroup5";
            // 
            // ribbonPage7
            // 
            ribbonPage7.Name = "ribbonPage7";
            ribbonPage7.Text = "ribbonPage7";
            // 
            // ribbonPageGroup6
            // 
            ribbonPageGroup6.ItemLinks.Add(barButMailSetup);
            ribbonPageGroup6.Name = "ribbonPageGroup6";
            ribbonPageGroup6.Text = "ribbonPageGroup6";
            // 
            // barButMailSetup
            // 
            barButMailSetup.Caption = "הגדרת דואר";
            barButMailSetup.Id = 78;
            barButMailSetup.Name = "barButMailSetup";
            // 
            // MainForm
            // 
            AllowFormGlass = DevExpress.Utils.DefaultBoolean.False;
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 650);
            Controls.Add(ribbonControl1);
            IconOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("MainForm.IconOptions.SvgImage");
            IsMdiContainer = true;
            Name = "MainForm";
            Ribbon = ribbonControl1;
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Text = "ניהול הנחות";
            WindowState = FormWindowState.Maximized;
            Load += MainForm_Load;
            ((System.ComponentModel.ISupportInitialize)repositoryItemTextEditYear).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemSpinEditSourceYear).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemSpinEditTargetYear).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemTextEditReport3Year).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemBtnRunReport3).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemTextEditReportYear).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemBtnRunReport).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemTextEditReport2Year).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemBtnRunReport2).EndInit();
            ((System.ComponentModel.ISupportInitialize)ribbonControl1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private BarButtonItem barBtnSaveAs;
        private BarButtonItem barButtonItem1;
        private RibbonPage ribbonPage4;
        private RibbonPage ribbonPage5;
        private RibbonMiniToolbar ribbonMiniToolbar1;
        private RibbonMiniToolbar ribbonMiniToolbar2;
        private RibbonControl ribbonControl1;
        private BarButtonItem barBtnPrint;
        private BarStaticItem barStaticUserName;
        private BarEditItem barEditYear;
        private BarEditItem barEditImportPrevYear;
        private BarEditItem barEditImportCurrYear;
        private BarEditItem barEditImportBillingYear;
        private RepositoryItemTextEdit repositoryItemTextEditYear;
        private BarButtonItem barBtnDiscountEligibility;
        private BarEditItem barEditSourceYear;
        private RepositoryItemSpinEdit repositoryItemSpinEditSourceYear;
        private BarEditItem barEditTargetYe;
        private RepositoryItemSpinEdit repositoryItemSpinEditTargetYear;
        private BarButtonItem barBtnCopyDiscounts;
        private BarStaticItem barStaticReportTitle;
        private BarEditItem barEditReportYear;
        private RepositoryItemTextEdit repositoryItemTextEditReportYear;
        private BarEditItem barBtnRunReport;
        private RepositoryItemButtonEdit repositoryItemBtnRunReport;
        private BarStaticItem barStaticReport2Title;
        private BarEditItem barEditReport2Year;
        private RepositoryItemTextEdit repositoryItemTextEditReport2Year;
        private BarEditItem barBtnRunReport2;
        private RepositoryItemButtonEdit repositoryItemBtnRunReport2;
        private BarStaticItem barStaticReport3Title;
        private BarEditItem barEditReport3Year;
        private RepositoryItemTextEdit repositoryItemTextEditReport3Year;
        private BarEditItem barBtnRunReport3;
        private RepositoryItemButtonEdit repositoryItemBtnRunReport3;
        private BarButtonItem barBtnSendSMS;
        private RibbonGalleryBarItem ribbonGalleryBarItem1;
        private BarButtonItem barButSysTables;
        private BarButtonItem barButDiscountCodes;
        private BarButtonItem barButCriterias;
        private BarButtonItem barButMinimumWage;
        private BarButtonItem barButParameters;
        private BarButtonItem barButAdmin;
        private BarButtonItem barButImportPreviouseYearData;
        private BarButtonItem barButImportCurrentYearData;
        private BarButtonItem barButtonItemYear2025;
        private BarSubItem barSubItemYear;
        private BarSubItem barSubItem1;
        private BarCheckItem barCheckItemPreviouseYear;
        private RibbonPage ribbonPage1;
        private RibbonPageGroup ribbonPageGroupWorkers;
        private RibbonPageGroup ribbonPageGroupSystem;
        private BarButtonItem barButConnection;
        private RibbonPageGroup ribbonPageGroupCopy;
        private RibbonPage ribbonPage2;
        private RibbonPageGroup ribbonPageGroup2;
        private RibbonPageGroup ribbonPageGroup1;
        private RibbonPage ribbonPageReports;
        //private RibbonPageGroup ribbonPageGroupReports;
        private RibbonPageGroup ribbonPageGroupReports;
        private RibbonPage ribbonPagePrint;
        private RibbonPageGroup ribbonPageGroupPrint;
        private RibbonPage ribbonPageSysTables;
        private RibbonPageGroup ribbonPageGroupSysTables;
        private RibbonPageGroup ribbonPageGroupDiscountCodes;
        private RibbonPageGroup ribbonPageGroup3;
        private RibbonPageGroup ribbonPageGroupMinimumWage;
        private RibbonPageGroup ribbonPageGroupParameters;
        private RibbonPage ribbonPageImportData;
        private RibbonPageGroup ribbonPageGroupImporCurrentYearData;
        private RibbonPageGroup ribbonPageGroupImportPreviouseYearData;
        private BarCheckItem barCheckItemCurrentYear;
        private BarSubItem barSubItem2;
        private BarButtonItem barButMultiDiscounts;
        private BarButtonItem barButReportPreviouseYear;
        //private BarButtonItem barButReportCurrentYear;
        private BarButtonItem barButReportCurrentYear;
        private RibbonPageGroup ribbonPageGroupImportBilling;
        private BarButtonItem barButImportBilling;
        private RibbonPageGroup ribbonPageGroupUserPermissions;
        private BarButtonItem barButtonItem2;
        private BarButtonItem barButImportRequestDates;
        private BarButtonItem barButUserPermissions;
        private RibbonPage ribbonPage3;
        private RibbonPageGroup ribbonPageGroup4;
        private RibbonPageGroup ribbonPageGroup5;
        private BarButtonItem barButMailSetup;
        private RibbonPageGroup ribbonPageGroup6;
        private RibbonPage ribbonPage7;
    }
}