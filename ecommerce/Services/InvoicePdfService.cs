using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Pearl_Poetry.Helpers;
using Pearl_Poetry.ViewModels.Order;

namespace Pearl_Poetry.Services
{
    public class InvoicePdfService
    {
        // site name from Admin > Settings
        private readonly SiteInfo _site;

        public InvoicePdfService(SiteInfo site)
        {
            _site = site;
        }

        public byte[] GenerateInvoice(OrderDetailsViewModel order)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Element(header => ComposeHeader(header, order));

                    page.Content()
                        .PaddingVertical(20)
                        .Element(content => ComposeContent(content, order));

                    page.Footer()
                        .Element(footer => ComposeFooter(footer));
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeHeader(
            IContainer container,
            OrderDetailsViewModel order)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(left =>
                    {
                        left.Item()
                            .Text(_site.Name.ToUpper())
                            .FontSize(24)
                            .Bold();

                        left.Item()
                            .Text("Jewelry & Fashion")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);

                        left.Item()
                            .PaddingTop(5)
                            .Text("Customer Invoice")
                            .FontSize(12)
                            .SemiBold();
                    });

                    row.ConstantItem(180).Column(right =>
                    {
                        right.Item()
                            .AlignRight()
                            .Text("INVOICE")
                            .FontSize(22)
                            .Bold();

                        right.Item()
                            .AlignRight()
                            .Text($"#{order.OrderNumber}")
                            .FontSize(11)
                            .SemiBold();

                        right.Item()
                            .AlignRight()
                            .Text($"Date: {order.CreatedAt:dd MMM yyyy}")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken1);
                    });
                });

                column.Item()
                    .PaddingTop(12)
                    .LineHorizontal(1);
            });
        }

        private void ComposeContent(
            IContainer container,
            OrderDetailsViewModel order)
        {
            container.Column(column =>
            {
                // Customer + Shipping Information
                column.Item()
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .PaddingRight(15)
                            .Element(c => ComposeCustomerInfo(c, order));

                        row.RelativeItem()
                            .PaddingLeft(15)
                            .Element(c => ComposeShippingInfo(c, order));
                    });

                column.Item()
                    .PaddingTop(25)
                    .Element(c => ComposeItemsTable(c, order));

                column.Item()
                    .PaddingTop(20)
                    .AlignRight()
                    .Element(c => ComposeSummary(c, order));

                if (!string.IsNullOrWhiteSpace(order.CustomerNote))
                {
                    column.Item()
                        .PaddingTop(20)
                        .Element(c =>
                        {
                            c.Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(10)
                                .Column(note =>
                                {
                                    note.Item()
                                        .Text("Customer Note")
                                        .Bold();

                                    note.Item()
                                        .PaddingTop(5)
                                        .Text(order.CustomerNote);
                                });
                        });
                }

                if (order.Payments.Any())
                {
                    column.Item()
                        .PaddingTop(20)
                        .Element(c => ComposePaymentInfo(c, order));
                }

                column.Item()
                    .PaddingTop(30)
                    .AlignCenter()
                    .Text($"Thank you for shopping with {_site.Name}!")
                    .FontSize(11)
                    .SemiBold();
            });
        }

        private void ComposeCustomerInfo(
            IContainer container,
            OrderDetailsViewModel order)
        {
            container.Column(column =>
            {
                column.Item()
                    .Text("CUSTOMER")
                    .FontSize(11)
                    .Bold();

                column.Item()
                    .PaddingTop(7)
                    .Text(order.CustomerName)
                    .SemiBold();

                if (!string.IsNullOrWhiteSpace(order.CustomerEmail))
                {
                    column.Item()
                        .Text(order.CustomerEmail);
                }

                if (!string.IsNullOrWhiteSpace(order.CustomerPhone))
                {
                    column.Item()
                        .Text(order.CustomerPhone);
                }
            });
        }

        private void ComposeShippingInfo(
            IContainer container,
            OrderDetailsViewModel order)
        {
            container.Column(column =>
            {
                column.Item()
                    .Text("BILLING / SHIPPING")
                    .FontSize(11)
                    .Bold();

                column.Item()
                    .PaddingTop(7)
                    .Text(order.ShippingFullName)
                    .SemiBold();

                column.Item()
                    .Text(order.ShippingPhone);

                if (!string.IsNullOrWhiteSpace(order.ShippingEmail))
                {
                    column.Item()
                        .Text(order.ShippingEmail);
                }

                column.Item()
                    .PaddingTop(4)
                    .Text(order.ShippingAddress);

                if (!string.IsNullOrWhiteSpace(order.ShippingDistrict))
                {
                    column.Item()
                        .Text(order.ShippingDistrict);
                }

                if (!string.IsNullOrWhiteSpace(order.ShippingPostalCode))
                {
                    column.Item()
                        .Text(order.ShippingPostalCode);
                }

                if (!string.IsNullOrWhiteSpace(order.ShippingCountry))
                {
                    column.Item()
                        .Text(order.ShippingCountry);
                }
            });
        }

        private void ComposeItemsTable(
            IContainer container,
            OrderDetailsViewModel order)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(60);
                    columns.ConstantColumn(80);
                });

                table.Header(header =>
                {
                    header.Cell()
                        .Element(HeaderCell)
                        .Text("#");

                    header.Cell()
                        .Element(HeaderCell)
                        .Text("Product");

                    header.Cell()
                        .Element(HeaderCell)
                        .Text("Unit Price");

                    header.Cell()
                        .Element(HeaderCell)
                        .Text("Qty");

                    header.Cell()
                        .Element(HeaderCell)
                        .AlignRight()
                        .Text("Total");
                });

                int index = 1;

                foreach (var item in order.Items)
                {
                    table.Cell()
                        .Element(BodyCell)
                        .Text(index.ToString());

                    table.Cell()
                        .Element(BodyCell)
                        .Column(product =>
                        {
                            product.Item()
                                .Text(item.ProductTitle)
                                .SemiBold();

                            if (!string.IsNullOrWhiteSpace(item.VariantLabel))
                            {
                                product.Item()
                                    .Text(item.VariantLabel)
                                    .FontSize(8)
                                    .FontColor(Colors.Grey.Darken1);
                            }
                        });

                    table.Cell()
                        .Element(BodyCell)
                        .Text($"৳ {item.Price:N2}");

                    table.Cell()
                        .Element(BodyCell)
                        .Text(item.Quantity.ToString());

                    table.Cell()
                        .Element(BodyCell)
                        .AlignRight()
                        .Text($"৳ {item.LineTotal:N2}");

                    index++;
                }
            });
        }

        private void ComposeSummary(
            IContainer container,
            OrderDetailsViewModel order)
        {
            container.Width(250)
                .Column(column =>
                {
                    column.Item()
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Text("Subtotal");

                            row.ConstantItem(100)
                                .AlignRight()
                                .Text($"৳ {order.Subtotal:N2}");
                        });

                    if (order.DiscountAmount > 0)
                    {
                        column.Item()
                            .PaddingTop(5)
                            .Row(row =>
                            {
                                row.RelativeItem()
                                    .Text("Discount");

                                row.ConstantItem(100)
                                    .AlignRight()
                                    .Text($"- ৳ {order.DiscountAmount:N2}");
                            });
                    }

                    column.Item()
                        .PaddingTop(5)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Text("Delivery Charge");

                            row.ConstantItem(100)
                                .AlignRight()
                                .Text($"৳ {order.DeliveryCharge:N2}");
                        });

                    column.Item()
                        .PaddingTop(8)
                        .LineHorizontal(1);

                    column.Item()
                        .PaddingTop(8)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Text("TOTAL")
                                .Bold()
                                .FontSize(13);

                            row.ConstantItem(100)
                                .AlignRight()
                                .Text($"৳ {order.TotalAmount:N2}")
                                .Bold()
                                .FontSize(13);
                        });
                });
        }

        private void ComposePaymentInfo(
            IContainer container,
            OrderDetailsViewModel order)
        {
            container.Column(column =>
            {
                column.Item()
                    .Text("PAYMENT INFORMATION")
                    .FontSize(11)
                    .Bold();

                foreach (var payment in order.Payments)
                {
                    column.Item()
                        .PaddingTop(7)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Text($"Method: {payment.PaymentMethod}");

                            row.RelativeItem()
                                .Text($"Status: {FormatStatus(payment.Status)}");

                            row.RelativeItem()
                                .AlignRight()
                                .Text($"৳ {payment.Amount:N2}");
                        });

                    if (!string.IsNullOrWhiteSpace(payment.TransactionId))
                    {
                        column.Item()
                            .Text($"Transaction ID: {payment.TransactionId}")
                            .FontSize(9);
                    }
                }
            });
        }

        private void ComposeFooter(IContainer container)
        {
            // read once here so the lambdas below can use it
            string siteName = _site.Name;

            container.Column(column =>
            {
                column.Item()
                    .LineHorizontal(1);

                column.Item()
                    .PaddingTop(7)
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1));
                        text.Span($"{siteName} • Thank you for your purchase");
                    });

                column.Item()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1));
                        text.Span("This is a computer-generated invoice.");
                    });

                column.Item()
                    .PaddingTop(3)
                    .AlignCenter()
                    .Element(text =>
                    {
                        text.Text(t =>
                        {
                            t.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1));
                            t.Span("Page ");
                            t.CurrentPageNumber();
                            t.Span(" of ");
                            t.TotalPages();
                        });
                    });
            });
        }

        private static IContainer HeaderCell(IContainer container)
        {
            return container
                .Background(Colors.Grey.Darken3)
                .Padding(7)
                .DefaultTextStyle(x =>
                    x.FontColor(Colors.White)
                     .Bold()
                     .FontSize(9));
        }

        private static IContainer BodyCell(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(7);
        }

        private static string FormatStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return "-";

            return string.Join(
                " ",
                status
                    .Split('_', StringSplitOptions.RemoveEmptyEntries)
                    .Select(word =>
                        char.ToUpper(word[0]) + word.Substring(1).ToLower()));
        }
    }
}