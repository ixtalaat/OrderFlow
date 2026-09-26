using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Seed;

public static class CatalogSeeder
{
    private sealed record SeedProduct(string Category, string Name, string Description, string Sku, decimal Price, int Stock);

    private static readonly SeedProduct[] Products =
    [
        new("مقابض الأبواب", "مقبض باب نحاسي كلاسيك", "مقبض باب نحاسي فاخر بتصميم كلاسيكي، مناسب للأبواب الخشبية الداخلية والخارجية.", "DOR-HDL-001", 950m, 40),
        new("مقابض الأبواب", "مقبض باب ستانلس حديث", "مقبض ستانلس ستيل مقاوم للصدأ بتصميم عصري للأبواب الحديثة.", "DOR-HDL-002", 720m, 55),
        new("مقابض الأبواب", "مقبض باب أسود مطفي", "مقبض أسود مطفي أنيق يضيف لمسة عصرية لأي باب خشبي أو معدني.", "DOR-HDL-003", 680m, 60),
        new("مقابض الأبواب", "مقبض باب ذهبي فاخر", "مقبض مطلي بالذهب بتصميم راقٍ للفلل والشقق الفاخرة.", "DOR-HDL-004", 1450m, 25),
        new("الأقفال", "قفل باب خشب سلندر", "سلندر نحاس عالي الأمان مع 3 مفاتيح للأبواب الخشبية.", "DOR-LCK-001", 550m, 70),
        new("الأقفال", "قفل باب حديد 7 لسان", "قفل حديد ثقيل بسبعة ألسنة لحماية قصوى لأبواب الشقق والمحلات.", "DOR-LCK-002", 890m, 45),
        new("الأقفال", "قفل ذكي ببصمة الإصبع", "قفل ذكي يفتح بالبصمة والكود والمفتاح، مع تنبيه عند انخفاض البطارية.", "DOR-LCK-003", 4800m, 15),
        new("الأقفال", "قفل مقبض بكرة للمكاتب", "قفل بمقبض بكرة عملي للأبواب الداخلية والمكاتب.", "DOR-LCK-004", 420m, 80),
        new("المفصلات", "مفصلة باب ستانلس 4 بوصة", "مفصلة ستانلس ستيل مقاس 4 بوصة تتحمل الاستخدام اليومي الثقيل.", "DOR-HNG-001", 95m, 200),
        new("المفصلات", "مفصلة نحاس ثقيلة", "مفصلة نحاسية للأبواب الثقيلة والرئيسية.", "DOR-HNG-002", 160m, 150),
        new("المفصلات", "مفصلة مخفية للأبواب الحديثة", "مفصلة مخفية غير ظاهرة تمنح الباب مظهراً نظيفاً وعصرياً.", "DOR-HNG-003", 220m, 90),
        new("المفصلات", "مفصلة زنبرك ذاتية الغلق", "مفصلة زنبرك تغلق الباب تلقائياً، مثالية للمطابخ والمخازن.", "DOR-HNG-004", 140m, 120),
        new("مقابض السحب", "مقبض سحب للزجاج السيكوريت", "مقبض سحب طويل من الستانلس لأبواب الزجاج السيكوريت.", "DOR-PUL-001", 1200m, 30),
        new("مقابض السحب", "مقبض سحب خشب زان", "مقبض سحب من خشب الزان الطبيعي بلمسة دافئة وأنيقة.", "DOR-PUL-002", 850m, 35),
        new("مقابض السحب", "مقبض سحب أسود للمحلات", "مقبض سحب أسود متين لواجهات المحلات والمكاتب.", "DOR-PUL-003", 980m, 28),
        new("ملحقات الأبواب", "عين سحرية بزاوية واسعة", "عين سحرية بزاوية رؤية 200 درجة لرؤية واضحة خارج الباب.", "DOR-ACC-001", 120m, 150),
        new("ملحقات الأبواب", "صدادة باب أرضية", "صدادة باب معدنية تحمي الحائط من اصطدام الباب.", "DOR-ACC-002", 65m, 300),
        new("ملحقات الأبواب", "سلسلة أمان للباب", "سلسلة أمان ستانلس تتيح فتح الباب جزئياً بأمان.", "DOR-ACC-003", 180m, 110),
        new("ملحقات الأبواب", "مسند باب مغناطيسي", "مسند مغناطيسي يثبت الباب مفتوحاً بشكل أنيق.", "DOR-ACC-004", 150m, 130),
        new("ملحقات الأبواب", "دفاية عتبة الباب", "شريط سفلي يمنع دخول التراب والهواء ويوفر العزل.", "DOR-ACC-005", 90m, 180),
        new("العوارض والمزالج", "مزلاج علوي ستانلس", "مزلاج علوي قوي لتأمين إضافي للأبواب والنوافذ.", "DOR-BLT-001", 110m, 160),
        new("العوارض والمزالج", "عارضة أمان للباب الرئيسي", "عارضة أفقية تمنع فتح الباب من الخارج تماماً.", "DOR-BLT-002", 240m, 75),
        new("العوارض والمزالج", "مزلاج أرضي مخفي", "مزلاج أرضي مخفي للأبواب المزدوجة.", "DOR-BLT-003", 130m, 100),
        new("العوارض والمزالج", "قفل سلسلة إضافي", "قفل سلسلة إضافي سهل التركيب لمزيد من الأمان.", "DOR-BLT-004", 95m, 140),
    ];

    public static async Task SeedAsync(ApplicationDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Products.AnyAsync(cancellationToken)) return;

        var categories = new Dictionary<string, Category>();
        foreach (var name in Products.Select(x => x.Category).Distinct())
        {
            var category = Category.Create(name);
            db.Categories.Add(category);
            categories[name] = category;
        }
        await db.SaveChangesAsync(cancellationToken);

        foreach (var seed in Products)
        {
            var product = Product.Create(seed.Name, seed.Description, seed.Sku, seed.Price, categories[seed.Category].Id);
            db.Products.Add(product);
            await db.SaveChangesAsync(cancellationToken);

            var inventory = Inventory.Create(product.Id);
            inventory.AddStock(seed.Stock);
            db.Inventories.Add(inventory);
        }
        await db.SaveChangesAsync(cancellationToken);
    }
}
