using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Date
{
    public enum TenorUnit {
        None, Sec, Min, Hour, Day, Week, Month, Year
    };

    public static class TenorUnitTools
    {
        public static TenorUnit GetTenorUnitFromChar(char input)
        {
            switch (input)
            {
                case 's':
                    return TenorUnit.Sec;
                case 'm':
                    return TenorUnit.Min;
                case 'H':
                    return TenorUnit.Hour;
                case 'D':
                    return TenorUnit.Day;
                case 'W':
                    return TenorUnit.Week;
                case 'M':
                    return TenorUnit.Month;
                case 'Y':
                    return TenorUnit.Year;
                default:
                    return TenorUnit.None;
            }
        }

        public static TenorUnit GetLowerUnit(this TenorUnit tu, bool skipWeek = false)
        {
            switch (tu)
            {
                case TenorUnit.Sec:
                    return TenorUnit.None;
                case TenorUnit.Min:
                    return TenorUnit.Sec;
                case TenorUnit.Hour:
                    return TenorUnit.Min;
                case TenorUnit.Day:
                    return TenorUnit.Hour;
                case TenorUnit.Week:
                    return TenorUnit.Day;
                case TenorUnit.Month:
                    if (skipWeek)
                        return TenorUnit.Day;
                    else
                        return TenorUnit.Week;
                case TenorUnit.Year:
                    return TenorUnit.Month;
                default:
                    return TenorUnit.None;
            }
        }
    }

    public class Tenor
    {
        private int _Number;
        private TenorUnit _Unit;

        public int Number { get { return _Number; } }
        public TenorUnit Unit { get { return _Unit; } }

        public Tenor(int nb, TenorUnit tu) { _Number = nb; _Unit = tu; }

        public Tenor(string tenorInput)
        {
            _Number = Int32.Parse(tenorInput.Substring(0, tenorInput.Length - 1));
            _Unit = TenorUnitTools.GetTenorUnitFromChar(tenorInput.LastOrDefault());
        }
    }

    public static class DateTimeTools
    {
        public static int GetNumberOfUnit(this DateTime dt, TenorUnit tnrUnit)
        {
            switch (tnrUnit)
            {
                case TenorUnit.Sec:
                    return dt.Second;
                case TenorUnit.Min:
                    return dt.Minute;
                case TenorUnit.Hour:
                    return dt.Hour;
                case TenorUnit.Day:
                    return dt.Day;
                case TenorUnit.Week:
                    return dt.Day / 7;
                case TenorUnit.Month:
                    return dt.Month;
                case TenorUnit.Year:
                    return dt.Year;
                default:
                    throw new Exception($"Unknown TenorUnit: [{tnrUnit}]");
            }
        }

        private static DateTime Aux_GetRoundDate(this DateTime dt, TenorUnit tu)
        {
            int nb_units = dt.GetNumberOfUnit(tu);
            switch (tu)
            {
                case TenorUnit.Hour:
                case TenorUnit.Min:
                case TenorUnit.Sec:
                    break;
                case TenorUnit.Month:
                case TenorUnit.Day:
                    nb_units -= 1;
                    break;
                case TenorUnit.Year:
                case TenorUnit.None:
                    nb_units = 0;
                    break;
            }
            dt = dt.AddTenor(new Tenor(-nb_units, tu));
            TenorUnit tuN = tu.GetLowerUnit(skipWeek: true);
            if (tuN == TenorUnit.None)
                return dt;
            else
                return dt.Aux_GetRoundDate(tuN);
        }

        public static DateTime GetRoundDate(this DateTime dt, TenorUnit tu)
        {
            if (tu == TenorUnit.Week)
            {
                int daysOffset = ((int)dt.DayOfWeek == 0 ? 7 : (int)dt.DayOfWeek) - 1;
                DateTime newDate = dt.AddDays(-daysOffset);
                return newDate.Aux_GetRoundDate(TenorUnit.Hour);
            }
            return dt.Aux_GetRoundDate(tu.GetLowerUnit(skipWeek: true));
        }

        public static DateTime AddTenor(this DateTime dt, Tenor tnr)
        {
            switch (tnr.Unit)
            {
                case TenorUnit.Sec:
                    return dt.AddSeconds(tnr.Number);
                case TenorUnit.Min:
                    return dt.AddMinutes(tnr.Number);
                case TenorUnit.Hour:
                    return dt.AddHours(tnr.Number);
                case TenorUnit.Day:
                    return dt.AddDays(tnr.Number);
                case TenorUnit.Week:
                    return dt.AddDays(tnr.Number * 7);
                case TenorUnit.Month:
                    return dt.AddMonths(tnr.Number);
                case TenorUnit.Year:
                    return dt.AddYears(tnr.Number);
                default:
                    throw new Exception($"Unknown TenorUnit: [{tnr.Unit}]");
            }
        }

        public static DateTime AddTenor(this DateTime dt, Tenor tnr, bool isRounded = false)
        {
            if (!isRounded) return dt.AddTenor(tnr);
            else
            {
                DateTime dt2 = dt.GetRoundDate(tnr.Unit);
                return dt2.AddTenor(tnr);
            }
        }

    }
}
