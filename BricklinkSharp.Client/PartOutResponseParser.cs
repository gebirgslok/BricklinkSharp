#region License
// Copyright (c) 2020 Jens Eisenbach
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace BricklinkSharp.Client;

internal static class PartOutResponseParser
{
    static class ErrorMessages
    {
        internal static string MatchInclItemsLotsFailed =>
            "Failed to match the included items and lot Regex in the received response.";

        internal static string MatchNumOfItemsFailed => "Failed to match '#of items' Regex within <b></b> tags.";

        internal static string MatchNumOfLotsFailed => "Failed to match '#of lots' Regex within <b></b> tags.";

        internal static string MatchNumberFailed => "Failed to match a 'number' Regex.";

        internal static string MatchDeciamlFailed => "Failed to match a 'decimal' Regex.";

        internal static string MatchTitleFailed => "Failed to match the title Regex in the received HTML response.";

        internal static string MatchPartOutValue => "Failed to match part-out-value Regex within <b></b> tags.";
    }

    static class Branches
    {
        internal static string Avg6MonthsSalesPrice => "Average 6 Months Sales Price";
        internal static string CurrentSalesAvgPrice => "Current Sales Average Price";
        internal static string InclNotInclItemsLots => "Included / Not Included Items and Lots";
    }

    static class Stages
    {
        internal static string InclItemsLots => "Match Included Items / Lots";
        internal static string NumOfItems => "Match #of Items";
        internal static string NumOfLots => "Match #of Lots";
        internal static string Number => "Match Number";
        internal static string Decimal => "Match Decimal";
        internal static string Title => "Match Title";
        internal static string PartOutValue => "Match Part-Out-Value";
    }

    class RegexMatchChainer
    {
        private readonly string _branch;
        private readonly string _url;
        private string _currentMatch;

        internal RegexMatchChainer(string s, string branch, string url)
        {
            _branch = branch;
            _url = url;
            _currentMatch = s;
        }

        internal string GetMatch() => _currentMatch;

        internal int GetMatchAsInt() => int.Parse(GetMatch());

        internal decimal GetMatchAsDecimal() => decimal.Parse(GetMatch(), _enUs);

        internal RegexMatchChainer Next(Regex regex, string message, string stage)
        {
            var m = regex.Match(_currentMatch);

            if (!m.Success)
            {
                throw new BricklinkPartOutParseErrorException(message, stage, _branch, _url);
            }

            _currentMatch = m.Value;
            return this;
        }
    }

    private static readonly Regex _partOut6MonthsSales = new Regex("<FONT CLASS=\"fv\">Average of last 6 months Sales:<\\/FONT>.+?<\\/B>",
        RegexOptions.IgnoreCase); 
    private static readonly Regex _partOutCurrentSales = new Regex("<FONT CLASS=\"fv\">Current Items For Sale Average:<\\/FONT>.+?<\\/B>",
        RegexOptions.IgnoreCase);
    private static readonly Regex _withinBTags = new Regex("(?<=<B>)(.*)(?=<\\/B>)", RegexOptions.IgnoreCase);
    private static readonly Regex _decimal = new Regex("\\d+\\.?\\d*");
    private static readonly Regex _number = new Regex("\\d+");
    private static readonly CultureInfo _enUs = new CultureInfo("en-US");
    private static readonly Regex _included = new Regex("<FONT CLASS=\"fv\">Including <B>\\d+<\\/B> Item[s]? in <B>\\d+<\\/B> Lot[s]?.", 
        RegexOptions.IgnoreCase);
    private static readonly Regex _numOfItems = new Regex("<B>\\d+<\\/B> Item[s]?", RegexOptions.IgnoreCase);
    private static readonly Regex _numOfLots = new Regex("<B>\\d+<\\/B> Lot[s]?", RegexOptions.IgnoreCase);
    private static readonly Regex _notIncluded = new Regex("<FONT COLOR=\"#FF0000\">Not Included <B>\\d+<\\/B> Item[s]? in <B>\\d+<\\/B> Lot[s]?.<\\/FONT>", 
        RegexOptions.IgnoreCase);
    private static readonly Regex _errorMessages = new Regex("<OL><LI>.*<\\/OL>", RegexOptions.IgnoreCase);

    private static (int includedItemsCount, int includedLotsCount, int notIncludedItemsCount, int notIncludedLotsCount) 
        ExtractIncluded(string htmlResponse, string branch, string url)
    {
        var includedHtml = new RegexMatchChainer(htmlResponse, branch, url)
            .Next(_included, ErrorMessages.MatchInclItemsLotsFailed, Stages.InclItemsLots)
            .GetMatch();

        var includedItemsCount = new RegexMatchChainer(includedHtml, branch, url)
            .Next(_numOfItems, ErrorMessages.MatchNumOfItemsFailed, Stages.NumOfItems)
            .Next(_number, ErrorMessages.MatchNumberFailed, Stages.Number)
            .GetMatchAsInt();
            
        var includedLotsCount = new RegexMatchChainer(includedHtml, branch, url)
            .Next(_numOfLots, ErrorMessages.MatchNumOfLotsFailed, Stages.NumOfLots)
            .Next(_number, ErrorMessages.MatchNumberFailed, Stages.Number)
            .GetMatchAsInt();

        var match = _notIncluded.Match(htmlResponse);

        if (match.Success)
        {
            var notIncludedItemsCount = new RegexMatchChainer(match.Value, branch, url)
                .Next(_numOfItems, ErrorMessages.MatchNumOfItemsFailed, Stages.NumOfItems)
                .Next(_number, ErrorMessages.MatchNumberFailed, Stages.Number)
                .GetMatchAsInt();

            var notIncludedLotsCount = new RegexMatchChainer(match.Value, branch, url)
                .Next(_numOfLots, ErrorMessages.MatchNumOfLotsFailed, Stages.NumOfLots)
                .Next(_number, ErrorMessages.MatchNumberFailed, Stages.Number)
                .GetMatchAsInt();

            return (includedItemsCount, includedLotsCount, notIncludedItemsCount, notIncludedLotsCount);
        }

        return (includedItemsCount, includedLotsCount, 0, 0);
    }

    private static decimal ExtractPrice(string htmlResponse, Regex regex, string branch, string url)
    {
        var price = new RegexMatchChainer(htmlResponse, branch, url)
            .Next(regex, Stages.Title, ErrorMessages.MatchTitleFailed)
            .Next(_withinBTags, Stages.PartOutValue, ErrorMessages.MatchPartOutValue)
            .Next(_decimal, Stages.Decimal, ErrorMessages.MatchDeciamlFailed)
            .GetMatchAsDecimal();

        return price;
    }

    private static string ExtractErrorMessage(string s)
    {
        const string startTag = "<OL><LI>";
        var startIndex = s.IndexOf(startTag, StringComparison.OrdinalIgnoreCase) + startTag.Length;
        var endIndex = s.IndexOf("</OL>", StringComparison.OrdinalIgnoreCase);
        return s.Substring(startIndex, endIndex - startIndex);
    }

    internal static PartOutResult ParseResponse(string htmlResponse, string url)
    {
        if (htmlResponse.Contains("CLASS=\"tb-global-error\">"))
        {
            var matches = _errorMessages.Matches(htmlResponse)
                .Cast<Match>()
                .Select(x => ExtractErrorMessage(x.Value));

            throw new BricklinkPartOutRequestErrorException(matches.ToList(), url);
        }

        var avg6MonthsSalesPrice = ExtractPrice(htmlResponse, _partOut6MonthsSales, Branches.Avg6MonthsSalesPrice, url);
        var currentSalesPrices = ExtractPrice(htmlResponse, _partOutCurrentSales, Branches.CurrentSalesAvgPrice, url);

        var (incItems, incLots, nIncItems, nIncLots) =
            ExtractIncluded(htmlResponse, Branches.InclNotInclItemsLots, url);

        return new PartOutResult(avg6MonthsSalesPrice, currentSalesPrices, incItems, incLots, nIncItems, nIncLots);
    }
}