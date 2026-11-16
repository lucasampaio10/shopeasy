using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShopEasy.BuildingBlocks.Domain;
using ShopEasy.Catalog.Domain.Products;

namespace ShopEasy.Catalog.Infrastructure.Persistence;

public static class CatalogSeeder
{
    /// <summary>Gives a fresh environment a small catalog so the order flow can be tried right away.</summary>
    public static async Task SeedCatalogAsync(this IHost host)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        if (await context.Products.AnyAsync())
        {
            return;
        }

        context.Products.AddRange(
            Product.Create("Teclado Mecânico K8", "KEY-K8", "Switches táteis, layout ABNT2 e conexão sem fio.", new Money(489.90m), 25),
            Product.Create("Mouse Sem Fio M3", "MOU-M3", "Sensor de 4000 DPI e bateria para 70 dias.", new Money(189.00m), 40),
            Product.Create("Monitor 27\" QHD", "MON-27Q", "Painel IPS de 165 Hz com suporte ajustável.", new Money(1899.00m), 8),
            Product.Create("Headset HX Cloud", "HEA-HX", "Áudio surround e microfone destacável.", new Money(349.50m), 15),
            Product.Create("Webcam Full HD", "WEB-FHD", "1080p a 60 fps com foco automático.", new Money(279.90m), 3),
            Product.Create("Cadeira Ergonômica Pro", "CHA-PRO", "Apoio lombar ajustável e encosto em tela.", new Money(1499.00m), 0));

        await context.SaveChangesAsync();
    }
}
