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

using BricklinkSharp.Client.Extensions;

namespace BricklinkSharp.Client;

internal static partial class Endpoints
{
    public static Endpoint<CatalogItem> CatalogItem(ItemType itemType, string itemNumber) =>
        Endpoint<CatalogItem>.Get(ApiResourceType.Item, $"items/{itemType.ToDomainString()}/{Segment(itemNumber)}");

    public static Endpoint<CatalogImage> CatalogImage(ItemType itemType, string itemNumber, int colorId) =>
        Endpoint<CatalogImage>.Get(ApiResourceType.ItemImage,
            $"items/{itemType.ToDomainString()}/{Segment(itemNumber)}/images/{colorId}");

    public static Endpoint<Superset[]> Supersets(ItemType itemType, string itemNumber, int colorId) =>
        Endpoint<Superset[]>.Get(ApiResourceType.Superset, $"items/{itemType.ToDomainString()}/{Segment(itemNumber)}/supersets",
                ResponseShape.ArrayNonEmpty)
            .WithQuery("color_id", ColorIdOrNull(colorId));

    public static Endpoint<Subset[]> Subsets(ItemType itemType, string itemNumber, int colorId,
        bool? includeOriginalBox, bool? includeInstruction, bool? breakMinifigs, bool? breakSubsets) =>
        Endpoint<Subset[]>.Get(ApiResourceType.Subset, $"items/{itemType.ToDomainString()}/{Segment(itemNumber)}/subsets",
                ResponseShape.ArrayNonEmpty)
            .WithQuery("color_id", ColorIdOrNull(colorId))
            .WithQuery("box", includeOriginalBox)
            .WithQuery("instruction", includeInstruction)
            .WithQuery("break_minifigs", breakMinifigs)
            .WithQuery("break_subsets", breakSubsets);

    public static Endpoint<PriceGuide> PriceGuide(ItemType itemType, string itemNumber, int colorId,
        PriceGuideSource? source, Condition? condition, string? countryCode, string? region,
        string? currencyCode, VatOption? vat) =>
        Endpoint<PriceGuide>.Get(ApiResourceType.PriceGuide, $"items/{itemType.ToDomainString()}/{Segment(itemNumber)}/price")
            .WithQuery("color_id", ColorIdOrNull(colorId))
            .WithQuery("guide_type", source?.ToDomainString())
            .WithQuery("new_or_used", condition?.ToDomainString())
            .WithQuery("country_code", countryCode)
            .WithQuery("region", region)
            .WithQuery("currency_code", currencyCode)
            .WithQuery("vat", vat?.ToDomainString());

    public static Endpoint<KnownColor[]> KnownColors(ItemType itemType, string itemNumber) =>
        Endpoint<KnownColor[]>.Get(ApiResourceType.KnownColor, $"items/{itemType.ToDomainString()}/{Segment(itemNumber)}/colors",
            ResponseShape.ArrayNonEmpty);

    public static Endpoint<Color[]> Colors() =>
        Endpoint<Color[]>.Get(ApiResourceType.ColorList, "colors", ResponseShape.ArrayNonEmpty);

    public static Endpoint<Color> Color(int colorId) =>
        Endpoint<Color>.Get(ApiResourceType.Color, $"colors/{colorId}");

    public static Endpoint<Category[]> Categories() =>
        Endpoint<Category[]>.Get(ApiResourceType.CategoryList, "categories", ResponseShape.ArrayNonEmpty);

    public static Endpoint<Category> Category(int categoryId) =>
        Endpoint<Category>.Get(ApiResourceType.Category, $"categories/{categoryId}");

    public static Endpoint<ItemMapping[]> ItemMappingsByItemNumber(string itemNumber, int? colorId) =>
        Endpoint<ItemMapping[]>.Get(ApiResourceType.ElementId, $"item_mapping/part/{Segment(itemNumber)}")
            .WithQuery("color_id", colorId);

    public static Endpoint<ItemMapping[]> ItemMappingsByElementId(string elementId) =>
        Endpoint<ItemMapping[]>.Get(ApiResourceType.ItemNumber, $"item_mapping/{Segment(elementId)}",
            ResponseShape.ArrayNonEmpty);
}
