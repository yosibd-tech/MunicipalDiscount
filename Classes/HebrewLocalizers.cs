using DevExpress.Utils.Filtering.Internal;
using DevExpress.XtraGrid.Localization;

namespace MunicipalDiscount.Classes
{
    /// <summary>
    /// תרגום מחרוזות הרשת של DevExpress לעברית.
    ///
    /// Applied once from Program.cs, so every grid in the application gets it —
    /// it used to be installed by frDiscountsCodes' constructor, which meant the
    /// rest of the app stayed English until that form happened to be opened.
    /// </summary>
    public class HebrewGridLocalizer : GridLocalizer
    {
        public override string GetLocalizedString(GridStringId id)
        {
            switch (id)
            {
                case GridStringId.GridNewRowText: return "לחץ כאן להוסיף שורה חדשה";
                case GridStringId.GridGroupPanelText: return "גרור לכאן כותרת עמודה כדי לקבץ לפיה";
                case GridStringId.MenuColumnSortAscending: return "מיין בסדר עולה";
                case GridStringId.MenuColumnSortDescending: return "מיין בסדר יורד";
                case GridStringId.MenuColumnClearFilter: return "נקה סינון";
                case GridStringId.MenuColumnFilterEditor: return "עורך הסינון...";
                case GridStringId.MenuColumnBestFit: return "התאם רוחב";
                case GridStringId.FindControlFindButton: return "חפש";
                case GridStringId.FindControlClearButton: return "נקה";
                case GridStringId.FindNullPrompt: return "הקלד טקסט לחיפוש...";
                case GridStringId.ColumnViewExceptionMessage: return "האם לתקן את הערך?";
                default: return base.GetLocalizedString(id);
            }
        }
    }

    /// <summary>
    /// תרגום חלון הסינון של העמודה (הסינון בסגנון אקסל) לעברית.
    ///
    /// These strings do NOT come from GridLocalizer — that is why the filter
    /// popup stayed English while the rest of the grid was translated. They live
    /// in FilterUIElementLocalizer, in DevExpress.Utils.Filtering.Internal.
    /// </summary>
    public class HebrewFilterLocalizer : FilterUIElementLocalizer
    {
        public override string GetLocalizedString(FilterUIElementLocalizerStringId id)
        {
            switch (id)
            {
                // הלשוניות והכפתורים של החלון
                case FilterUIElementLocalizerStringId.FilteringUITabValues: return "ערכים";
                case FilterUIElementLocalizerStringId.FilteringUITabGroups: return "קבוצות";
                case FilterUIElementLocalizerStringId.FilteringUIClearFilter: return "נקה סינון";
                case FilterUIElementLocalizerStringId.FilteringUIClose: return "סגור";
                case FilterUIElementLocalizerStringId.FilteringUIMoreButtonCaption: return "עוד...";
                case FilterUIElementLocalizerStringId.FilteringUIFewerButtonCaption: return "פחות...";
                case FilterUIElementLocalizerStringId.FilteringUISearchByYearCaption: return "שנה";
                case FilterUIElementLocalizerStringId.FilteringUISearchByMonthCaption: return "חודש";
                case FilterUIElementLocalizerStringId.FilteringUISearchByDayCaption: return "יום";

                // שמות קבוצות התנאים
                case FilterUIElementLocalizerStringId.CustomUIFiltersNumericName: return "סינון מספרים";
                case FilterUIElementLocalizerStringId.CustomUIFiltersTextName: return "סינון טקסט";
                case FilterUIElementLocalizerStringId.CustomUIFiltersDateTimeName: return "סינון תאריכים";
                case FilterUIElementLocalizerStringId.CustomUIFiltersTimeName: return "סינון שעות";
                case FilterUIElementLocalizerStringId.CustomUIFiltersDurationName:
                case FilterUIElementLocalizerStringId.CustomUIFiltersBooleanName:
                case FilterUIElementLocalizerStringId.CustomUIFiltersEnumName: return "סינון";

                // התנאים עצמם
                case FilterUIElementLocalizerStringId.CustomUIFilterEqualsName: return "שווה ל";
                case FilterUIElementLocalizerStringId.CustomUIFilterDoesNotEqualName: return "שונה מ";
                case FilterUIElementLocalizerStringId.CustomUIFilterBetweenName: return "בין";
                case FilterUIElementLocalizerStringId.CustomUIFilterGreaterThanName: return "גדול מ";
                case FilterUIElementLocalizerStringId.CustomUIFilterGreaterThanOrEqualToName: return "גדול או שווה ל";
                case FilterUIElementLocalizerStringId.CustomUIFilterLessThanName: return "קטן מ";
                case FilterUIElementLocalizerStringId.CustomUIFilterLessThanOrEqualToName: return "קטן או שווה ל";
                case FilterUIElementLocalizerStringId.CustomUIFilterIsNullName: return "ריק";
                case FilterUIElementLocalizerStringId.CustomUIFilterIsNotNullName: return "אינו ריק";
                case FilterUIElementLocalizerStringId.CustomUIFilterIsBlankName: return "ריק";
                case FilterUIElementLocalizerStringId.CustomUIFilterIsNotBlankName: return "אינו ריק";
                case FilterUIElementLocalizerStringId.CustomUIFilterContainsName: return "מכיל";
                case FilterUIElementLocalizerStringId.CustomUIFilterDoesNotContainName: return "אינו מכיל";
                case FilterUIElementLocalizerStringId.CustomUIFilterBeginsWithName: return "מתחיל ב";
                case FilterUIElementLocalizerStringId.CustomUIFilterEndsWithName: return "מסתיים ב";
                case FilterUIElementLocalizerStringId.CustomUIFilterDoesNotBeginWithName:
                case FilterUIElementLocalizerStringId.CustomUIFilterDoesNotBeginsWithName: return "אינו מתחיל ב";
                case FilterUIElementLocalizerStringId.CustomUIFilterDoesNotEndWithName:
                case FilterUIElementLocalizerStringId.CustomUIFilterDoesNotEndsWithName: return "אינו מסתיים ב";
                case FilterUIElementLocalizerStringId.CustomUIFilterLikeName: return "דומה ל";
                case FilterUIElementLocalizerStringId.CustomUIFilterNotLikeName: return "אינו דומה ל";
                case FilterUIElementLocalizerStringId.CustomUIFilterTopNName: return "הגדולים ביותר";
                case FilterUIElementLocalizerStringId.CustomUIFilterBottomNName: return "הקטנים ביותר";
                case FilterUIElementLocalizerStringId.CustomUIFilterAboveAverageName: return "מעל הממוצע";
                case FilterUIElementLocalizerStringId.CustomUIFilterBelowAverageName: return "מתחת לממוצע";
                case FilterUIElementLocalizerStringId.CustomUIFilterInRangeName: return "בטווח ערכים";
                case FilterUIElementLocalizerStringId.CustomUIFilterCustomName: return "סינון מותאם";
                case FilterUIElementLocalizerStringId.CustomUIFilterNoneName: return "בחר תנאי";

                // תאריכים
                case FilterUIElementLocalizerStringId.CustomUIFilterIsSameDayName: return "באותו תאריך";
                case FilterUIElementLocalizerStringId.CustomUIFilterBeforeName: return "לפני";
                case FilterUIElementLocalizerStringId.CustomUIFilterAfterName: return "אחרי";
                case FilterUIElementLocalizerStringId.CustomUIFilterInDateRangeName: return "בטווח תאריכים";
                case FilterUIElementLocalizerStringId.CustomUIFilterTodayName: return "היום";
                case FilterUIElementLocalizerStringId.CustomUIFilterYesterdayName: return "אתמול";
                case FilterUIElementLocalizerStringId.CustomUIFilterTomorrowName: return "מחר";
                case FilterUIElementLocalizerStringId.CustomUIFilterThisWeekName: return "השבוע";
                case FilterUIElementLocalizerStringId.CustomUIFilterLastWeekName: return "שבוע שעבר";
                case FilterUIElementLocalizerStringId.CustomUIFilterNextWeekName: return "שבוע הבא";
                case FilterUIElementLocalizerStringId.CustomUIFilterThisMonthName: return "החודש";
                case FilterUIElementLocalizerStringId.CustomUIFilterLastMonthName: return "חודש שעבר";
                case FilterUIElementLocalizerStringId.CustomUIFilterNextMonthName: return "חודש הבא";
                case FilterUIElementLocalizerStringId.CustomUIFilterThisYearName: return "השנה";
                case FilterUIElementLocalizerStringId.CustomUIFilterLastYearName: return "שנה שעברה";
                case FilterUIElementLocalizerStringId.CustomUIFilterNextYearName: return "שנה הבאה";
                case FilterUIElementLocalizerStringId.CustomUIFilterYearToDateName: return "מתחילת השנה";
                case FilterUIElementLocalizerStringId.CustomUIFilterDatePeriodsName: return "תקופות תאריך";

                // תוויות ושדות הזנה
                case FilterUIElementLocalizerStringId.CustomUIValueLabel: return "ערך";
                case FilterUIElementLocalizerStringId.CustomUITypeLabel: return "סוג";
                case FilterUIElementLocalizerStringId.CustomUIFirstLabel: return "ראשון";
                case FilterUIElementLocalizerStringId.CustomUISecondLabel: return "שני";
                case FilterUIElementLocalizerStringId.CustomUINullValuePromptChooseOne: return "בחר...";
                case FilterUIElementLocalizerStringId.CustomUINullValuePromptEnterAValue: return "הזן ערך...";
                case FilterUIElementLocalizerStringId.CustomUINullValuePromptSelectAValue: return "בחר ערך...";
                case FilterUIElementLocalizerStringId.CustomUINullValuePromptEnterADate: return "הזן תאריך...";
                case FilterUIElementLocalizerStringId.CustomUINullValuePromptSelectADate: return "בחר תאריך...";
                case FilterUIElementLocalizerStringId.CustomUINullValuePromptEnterATime: return "הזן שעה...";
                case FilterUIElementLocalizerStringId.CustomUINullValuePromptSelectATime: return "בחר שעה...";
                case FilterUIElementLocalizerStringId.CustomUINullValuePromptSearchControl: return "הקלד טקסט לחיפוש...";

                default: return base.GetLocalizedString(id);
            }
        }
    }
}
