using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RestaurantPlatform.Models;
using System.Globalization;

namespace RestaurantPlatform.Pdf
{
    public class InvoiceDocument : IDocument
    {
        private readonly Order _order;

        public InvoiceDocument(Order order)
        {
            _order = order;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(12));
                page.Header().Text($"Фактура за поръчка №{_order.Id}").FontSize(18).Bold().AlignCenter();

                page.Content().Column(col =>
                {
                    col.Item().Text($"Дата: {_order.OrderDate.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)}");
                    col.Item().Text($"Потребител ID: {_order.UserId}");
                    col.Item().Text($"Статус: {_order.Status}");

                    col.Item().PaddingVertical(10).Element(ComposeItemsTable);

                    decimal total = _order.Items.Sum(i => i.Quantity * i.Dish.Price);
                    col.Item().AlignRight().Text($"Обща сума: {total.ToString("0.00")} лв.").Bold();
                });

                page.Footer().AlignCenter().Text("Благодарим ви, че избрахте нашия ресторант!");
            });
        }

        void ComposeItemsTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(5); // Ястие
                    columns.RelativeColumn(2); // Количество
                    columns.RelativeColumn(3); // Цена
                });

                table.Header(header =>
                {
                    header.Cell().Text("Ястие").Bold();
                    header.Cell().Text("Количество").Bold();
                    header.Cell().Text("Цена (лв.)").Bold();
                });

                foreach (var item in _order.Items)
                {
                    table.Cell().Text(item.Dish.Name);
                    table.Cell().Text(item.Quantity.ToString());
                    table.Cell().Text((item.Dish.Price * item.Quantity).ToString("0.00"));
                }
            });
        }
    }
}
