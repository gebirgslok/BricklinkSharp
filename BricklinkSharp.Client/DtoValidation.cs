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
using System.Collections.Generic;

namespace BricklinkSharp.Client;

internal static class DtoValidation
{
    internal static void Validate(NewInventory dto)
    {
        var dtoType = dto.GetType();

        if (dto.Item.Type != ItemType.Set && dto.Completeness != Completeness.Complete)
        {
            throw new BricklinkInvalidParameterException(new Dictionary<string, object>
                {
                    {"Item.Type", dto.Item.Type},
                    {nameof(dto.Completeness), dto.Completeness}
                },
                $"{dto.Completeness} can only be applied to items of type {ItemType.Set}.",
                dtoType);
        }

        ValidateStockRoomId(dto.IsStockRoom, dto.StockRoomId, dtoType);

        TierRules.Validate(dtoType,
            quantityTiers: new[]
            {
                new TierField(nameof(dto.TierQuantity1), dto.TierQuantity1, dto.TierQuantity1, dto.TierQuantity1 > 0),
                new TierField(nameof(dto.TierQuantity2), dto.TierQuantity2, dto.TierQuantity2, dto.TierQuantity2 > 0),
                new TierField(nameof(dto.TierQuantity3), dto.TierQuantity3, dto.TierQuantity3, dto.TierQuantity3 > 0)
            },
            priceTiers: new[]
            {
                new TierField(nameof(dto.TierPrice1), dto.TierPrice1, dto.TierPrice1, dto.TierPrice1 > 0.0M),
                new TierField(nameof(dto.TierPrice2), dto.TierPrice2, dto.TierPrice2, dto.TierPrice2 > 0.0M),
                new TierField(nameof(dto.TierPrice3), dto.TierPrice3, dto.TierPrice3, dto.TierPrice3 > 0.0M)
            },
            arePricesDescending: true,
            maxFirstTierPriceName: nameof(dto.UnitPrice),
            maxFirstTierPrice: dto.UnitPrice);

        ValidateBulk(dto.BulkQuantity, dtoType);
    }

    internal static void Validate(UpdateInventory dto)
    {
        var dtoType = dto.GetType();

        ValidateStockRoomId(dto.IsStockRoom == true, dto.StockRoomId, dtoType);

        // A partial update cannot know the base price, so the TierPrice1 < UnitPrice rule only applies
        // when UnitPrice is part of the same update (a null maxFirstTierPrice skips it).
        TierRules.Validate(dtoType,
            quantityTiers: new[]
            {
                new TierField(nameof(dto.TierQuantity1), dto.TierQuantity1, dto.TierQuantity1.GetValueOrDefault(), dto.TierQuantity1 != null),
                new TierField(nameof(dto.TierQuantity2), dto.TierQuantity2, dto.TierQuantity2.GetValueOrDefault(), dto.TierQuantity2 != null),
                new TierField(nameof(dto.TierQuantity3), dto.TierQuantity3, dto.TierQuantity3.GetValueOrDefault(), dto.TierQuantity3 != null)
            },
            priceTiers: new[]
            {
                new TierField(nameof(dto.TierPrice1), dto.TierPrice1, dto.TierPrice1.GetValueOrDefault(), dto.TierPrice1 != null),
                new TierField(nameof(dto.TierPrice2), dto.TierPrice2, dto.TierPrice2.GetValueOrDefault(), dto.TierPrice2 != null),
                new TierField(nameof(dto.TierPrice3), dto.TierPrice3, dto.TierPrice3.GetValueOrDefault(), dto.TierPrice3 != null)
            },
            arePricesDescending: true,
            maxFirstTierPriceName: nameof(dto.UnitPrice),
            maxFirstTierPrice: dto.UnitPrice);

        ValidateBulk(dto.BulkQuantity, dtoType);
    }

    internal static void Validate(CouponTerms dto)
    {
        if (dto.DiscountType != DiscountType.Percentage)
        {
            return;
        }

        TierRules.Validate(dto.GetType(),
            quantityTiers: new[]
            {
                new TierField(nameof(dto.TierDiscountRate1), dto.TierDiscountRate1, dto.TierDiscountRate1, dto.TierDiscountRate1 > 0),
                new TierField(nameof(dto.TierDiscountRate2), dto.TierDiscountRate2, dto.TierDiscountRate2, dto.TierDiscountRate2 > 0),
                new TierField(nameof(dto.TierDiscountRate3), dto.TierDiscountRate3, dto.TierDiscountRate3, dto.TierDiscountRate3 > 0)
            },
            priceTiers: new[]
            {
                new TierField(nameof(dto.TierPrice1), dto.TierPrice1, dto.TierPrice1, dto.TierPrice1 > 0.0M),
                new TierField(nameof(dto.TierPrice2), dto.TierPrice2, dto.TierPrice2, dto.TierPrice2 > 0.0M),
                new TierField(nameof(dto.TierPrice3), dto.TierPrice3, dto.TierPrice3, dto.TierPrice3 > 0.0M)
            },
            arePricesDescending: false,
            maxFirstTierPriceName: null,
            maxFirstTierPrice: null);
    }

    private static void ValidateStockRoomId(bool isStockRoom, string? stockRoomId, Type dtoType)
    {
        if (isStockRoom && string.IsNullOrEmpty(stockRoomId))
        {
            throw new BricklinkInvalidParameterException(new Dictionary<string, object>
                {
                    {"StockRoomId", "Null"}
                },
                "StockRoomId cannot be Null if IsStockRoom is true.",
                dtoType);
        }

        if (isStockRoom && stockRoomId != "A" && stockRoomId != "B" && stockRoomId != "C")
        {
            throw new BricklinkInvalidParameterException(new Dictionary<string, object>
                {
                    {"StockRoomId", stockRoomId!}
                },
                $"StockRoomId must be either A, B or C (set to: {stockRoomId}.",
                dtoType);
        }
    }

    private static void ValidateBulk(int? bulk, Type dtoType)
    {
        if (bulk.HasValue && bulk.Value < 1)
        {
            throw new BricklinkInvalidParameterException(new Dictionary<string, object>
                {
                    {"BulkQuantity", bulk}
                },
                "BulkQuantity must be > 0.",
                dtoType);
        }
    }

    private readonly struct TierField
    {
        internal TierField(string name, object? reportedValue, decimal comparableValue, bool isProvided)
        {
            Name = name;
            ReportedValue = reportedValue;
            ComparableValue = comparableValue;
            IsProvided = isProvided;
        }

        internal string Name { get; }
        internal object? ReportedValue { get; }
        internal decimal ComparableValue { get; }
        internal bool IsProvided { get; }
    }

    private static class TierRules
    {
        private const int TierCount = 3;

        internal static void Validate(Type dtoType,
            TierField[] quantityTiers,
            TierField[] priceTiers,
            bool arePricesDescending,
            string? maxFirstTierPriceName,
            decimal? maxFirstTierPrice)
        {
            var providedFieldCount = CountSet(quantityTiers) + CountSet(priceTiers);

            if (providedFieldCount > 0 && providedFieldCount < 2 * TierCount)
            {
                var allTierFields = new Dictionary<string, object>();
                foreach (var field in ChainFields(quantityTiers, priceTiers))
                {
                    allTierFields.Add(field.Name, field.ReportedValue!);
                }

                throw new BricklinkInvalidParameterException(allTierFields,
                    $"All properties ({priceTiers[0].Name}-{priceTiers[TierCount - 1].Name}, {quantityTiers[0].Name}-{quantityTiers[TierCount - 1].Name}) must be set" +
                    "when using tiered pricing.",
                    dtoType);
            }

            if (providedFieldCount != 2 * TierCount)
            {
                return;
            }

            var invalidParameters = new Dictionary<string, object>();

            for (var i = 1; i < TierCount; i++)
            {
                if (quantityTiers[i].ComparableValue <= quantityTiers[i - 1].ComparableValue)
                {
                    invalidParameters.Add(quantityTiers[i].Name, quantityTiers[i].ReportedValue!);
                }
            }

            if (arePricesDescending && maxFirstTierPrice.HasValue && priceTiers[0].ComparableValue >= maxFirstTierPrice.Value)
            {
                invalidParameters.Add(priceTiers[0].Name, priceTiers[0].ReportedValue!);
            }

            for (var i = 1; i < TierCount; i++)
            {
                var violatesOrder = arePricesDescending
                    ? priceTiers[i].ComparableValue >= priceTiers[i - 1].ComparableValue
                    : priceTiers[i].ComparableValue <= priceTiers[i - 1].ComparableValue;

                if (violatesOrder)
                {
                    invalidParameters.Add(priceTiers[i].Name, priceTiers[i].ReportedValue!);
                }
            }

            if (invalidParameters.Count > 0)
            {
                throw new BricklinkInvalidParameterException(invalidParameters,
                    "Following rules apply for tiered pricing: " + Environment.NewLine +
                    DescribePriceOrder(priceTiers, arePricesDescending, maxFirstTierPriceName) + "," + Environment.NewLine +
                    $"{quantityTiers[0].Name} < {quantityTiers[1].Name} < {quantityTiers[2].Name}.",
                    dtoType);
            }
        }

        private static string DescribePriceOrder(TierField[] priceTiers, bool arePricesDescending, string? maxFirstTierPriceName)
        {
            var separator = arePricesDescending ? " > " : " < ";
            var chain = string.Join(separator, priceTiers[0].Name, priceTiers[1].Name, priceTiers[2].Name);
            return arePricesDescending ? maxFirstTierPriceName + separator + chain : chain;
        }

        private static int CountSet(TierField[] fields)
        {
            var count = 0;
            foreach (var field in fields)
            {
                if (field.IsProvided)
                {
                    count++;
                }
            }

            return count;
        }

        private static IEnumerable<TierField> ChainFields(TierField[] first, TierField[] second)
        {
            foreach (var field in first)
            {
                yield return field;
            }

            foreach (var field in second)
            {
                yield return field;
            }
        }
    }
}
