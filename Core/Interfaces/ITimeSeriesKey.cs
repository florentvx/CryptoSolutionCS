﻿using Core.Quotes;
using Core.TimeSeriesKeys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ITimeSeriesKey
    {
        string GetTimeSeriesKey();
        string GetFullName();
        TimeSeriesKeyType GetKeyType();
        List<CurrencyPair> GetCurrencyPairs();
    }
}