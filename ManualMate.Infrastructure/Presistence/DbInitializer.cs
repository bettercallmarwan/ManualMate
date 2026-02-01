using ManualMate.Domain.Models;

namespace ManualMate.Infrastructure.Presistence
{
    public static class DbInitializer
    {
        public static async Task seedAsync(ManualMateDbContext context)
        {
            if (!context.Items.Any())
            {
                var items = new List<Item>
                {
                    new Item
                    {
                        Name = "Samsung Galaxy S24",
                        Description = "A series of high-end Android-based smartphones developed, manufactured, and marketed by Samsung Electronics as part of its flagship Galaxy S series",
                        FilePath = "Files/Galaxy_S24_Manual.pdf",
                        LastUpdated = DateTime.UtcNow
                    },
                    new Item
                    {
                        Name = "PS5",
                        Description = "The PlayStation 5 (PS5) is a home video game console developed by Sony Interactive Entertainment. It was announced as the successor to the PlayStation 4 in April 2019, was launched on November 12, 2020",
                        FilePath = "Files/PS5_Manual.pdf",
                        LastUpdated = DateTime.UtcNow
                    },
                    new Item
                    {
                        Name = "Sharp AC",
                        Description = "Sharp air conditioners are Japanese-made electronic devices known for features like Plasmacluster ion technology to purify the air, Gentle Air Cool (Coanda) mode that directs air to the ceiling to prevent cold drafts, and energy-efficient Inverter technology",
                        FilePath = "Files/Sharp_Ac_Manual.pdf",
                        LastUpdated = DateTime.UtcNow
                    }
                };

                context.Items.AddRange(items);
                await context.SaveChangesAsync();
            }
        }
    }
}
