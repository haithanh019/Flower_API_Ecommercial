using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
    public record ProductVm(
        int Id,
        string Name,
        string Image,
        decimal Price,
        int Rating,
        string Badge
    );

    public List<ProductVm> Products { get; set; } = new();

    public void OnGet()
    {
        var names = new[]
        {
            "Hồng phấn dịu",
            "Nắng xuân",
            "Tulip tinh khôi",
            "Lavender mộng mơ",
            "Mẫu đơn kiêu sa",
            "Cúc họa mi",
            "Hướng dương rực rỡ",
            "Baby trắng muốt",
        };

        for (int i = 0; i < names.Length; i++)
        {
            var name = names[i];
            var price = GetStableNumber(name, 350_000, 950_000); // 350k–950k
            var rating = GetStableNumber(name + "*", 4, 5); // 4–5 sao
            var badge =
                (i % 3 == 0) ? "Mới"
                : (i % 3 == 1) ? "Bán chạy"
                : string.Empty;

            Products.Add(
                new ProductVm(
                    Id: i + 1,
                    Name: name,
                    Image: $"/images/flowers/sample-{(i % 4) + 1}.jpg",
                    Price: price,
                    Rating: rating,
                    Badge: badge
                )
            );
        }
    }

    // Hash -> số ổn định trong [min, max]
    private static int GetStableNumber(string input, int min, int max)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        int val = Math.Abs(BitConverter.ToInt32(bytes, 0));
        return min + (val % (max - min + 1));
    }
}
