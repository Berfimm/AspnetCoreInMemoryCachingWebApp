using InMemoryCachingApp.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace InMemoryCachingApp.Web.Controllers
{
    public class ProductController : Controller
    {
        private IMemoryCache _memoryCache;
        public ProductController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public IActionResult Index()
        {

            //her türlü data tutabiliriz. Memory deki hafıza miktarı ve datanın serialize edilmesi önemli
            //_memoryCache.Set<string>("zaman", DateTime.Now.ToString());
            //if(string.IsNullOrEmpty(_memoryCache.Get<string>("zaman")))
            //{
            //    _memoryCache.Set<string>("zaman", DateTime.Now.ToString());
            //}

            if (!_memoryCache.TryGetValue("zaman",out string zamancache)) // eğer bu keye sahip olan datayı alabiliyorsa değişken (zamancache) üzerinden erişmeye çalışacak
            {
                // best practise her ikisinin de aynı anada kullanılması, bayat data alınmaması
                MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();
                options.AbsoluteExpiration = DateTime.Now.AddSeconds(10); // belirlenen süre kadar görünür
               //options.SlidingExpiration = TimeSpan.FromSeconds(10); // verilen süre içerisinde işlem yapılırsa o süre kadar erişilebilir. ama işlem yapılmazsa süre bittiğinde gider
                options.Priority = CacheItemPriority.High; // cache deki sıralama, neverremove kullanmak mantıklı değil çünkü memory dolduğunda hataya düşebilir.
                options.RegisterPostEvictionCallback((key, value, reason, state) => {  // silinme nedenlerinin tutlmasını sağlayan metot
                    _memoryCache.Set("callback", $"{key}-> {value} => sebep: {reason}");
                });
                _memoryCache.Set<string>("zaman", DateTime.Now.ToString(),options);
            }
            //var x = zamancache;


            //Complex types -- ınmemory de serialize etmeye gerek yok
            Product p = new Product { Id = 1, Name = "Kalem", Price = 200 };

            _memoryCache.Set<Product>("product1", p);
            return View();
        }
        public IActionResult Show()
        {

            //_memoryCache.Remove("zaman");// sil
            //_memoryCache.GetOrCreate<string>("zaman", entry => // varsa al yoksa oluştur
            //{
            //    //entry üzerinden diğer işlemleri yapabiliriz.
            //    return DateTime.Now.ToString();
            //});
            _memoryCache.TryGetValue("zaman", out string zamancache);

            _memoryCache.TryGetValue("callback", out string callback);
            ViewBag.zaman = zamancache;
            ViewBag.callback = callback; // belirlenen süre bittiğinde bu görünecek
            ViewBag.product = _memoryCache.Get<Product>("product1");
            return View();
        }
    }
}
