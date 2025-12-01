using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers; // <-- cần gói DotNetSeleniumExtras.WaitHelpers
using System;

static class DomHelpers
{
    public static IWebElement SafeFind(this IWebDriver d, By by, int timeoutSec = 12)
    {
        return new WebDriverWait(d, TimeSpan.FromSeconds(timeoutSec))
            .Until(ExpectedConditions.ElementExists(by));
    }

    public static IWebElement SafeClickable(this IWebDriver d, By by, int timeoutSec = 12)
    {
        return new WebDriverWait(d, TimeSpan.FromSeconds(timeoutSec))
            .Until(ExpectedConditions.ElementToBeClickable(by));
    }

    public static void ScrollIntoViewAndClick(this IWebDriver d, IWebElement el)
    {
        try
        {
            ((IJavaScriptExecutor)d).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", el);
            el.Click();
        }
        catch
        {
            ((IJavaScriptExecutor)d).ExecuteScript("arguments[0].click();", el); // fallback
        }
    }
}

class Program
{
    static void Main()
    {
        var options = new ChromeOptions();
        // options.AddArgument("--headless=new");
        options.AddArgument("--start-maximized");

        using var driver = new ChromeDriver(options);

        try
        {
            driver.Navigate().GoToUrl("https://www.savor.vn/");
            TryClosePopups(driver);

            // PHẦN 1: Thêm sản phẩm vào giỏ
            ThemGioHang(driver);

            // PHẦN 2: Đặt 1 chiếc bánh hỏa tốc
            DatHang(driver);

            Console.WriteLine("Hoàn thành 2 tác vụ theo yêu cầu đề.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("LỖI: " + ex.Message);
        }
        finally
        {
            driver.Quit();
        }
    }

    // ============================
    // HÀM 1: THÊM 1 SẢN PHẨM VÀO GIỎ
    // ============================
    static void ThemGioHang(IWebDriver driver)
    {
        // Đi thẳng tới 1 trang sản phẩm cụ thể (có thể đổi URL khác nếu sản phẩm thay đổi)
        driver.Navigate().GoToUrl("https://www.savor.vn/products/combo-best-seller/");
        TryClosePopups(driver);

        // Nút "Thêm vào giỏ" – dò theo text/thuộc tính phổ biến
        By addToCartBy = By.XPath("//button[(contains(.,'Thêm vào giỏ'))]");
        var addToCartBtn = driver.SafeClickable(addToCartBy, 12);
        driver.ScrollIntoViewAndClick(addToCartBtn);

        // Mở giỏ/checkout để xác nhận
        driver.Navigate().GoToUrl("https://www.savor.vn/checkout");
        var pageText = driver.PageSource;
    }

    // ============================
    // HÀM 2: ĐẶT 1 CHIẾC BÁNH HỎA TỐC
    // ============================

    static void DatHang(IWebDriver d)
    {
        d.Navigate().GoToUrl("https://www.savor.vn/checkout");
        TryClosePopups(d);

        var w = new WebDriverWait(d, TimeSpan.FromSeconds(15));
        var js = (IJavaScriptExecutor)d;
        var ac = new OpenQA.Selenium.Interactions.Actions(d);

        // 1. Họ tên + SĐT
        w.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='orderCustomer.name']")))
         .SendKeys("Nguyen Van A");
        w.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='orderCustomer.cellphone']")))
         .SendKeys("0912345678");

        // Helper: tick checkbox theo text
        void Check(string text)
        {
            var e = w.Until(ExpectedConditions.ElementToBeClickable(
                By.XPath($"//label[normalize-space()='{text}']/ancestor::div[contains(@class,'leading-none')]/preceding-sibling::button[@role='checkbox']")));
            js.ExecuteScript("arguments[0].scrollIntoView({block:'center'});", e);
            e.Click();
        }
        Check("Giống người đặt hàng");
        Check("Lấy tại cửa hàng");
        // Helper: mở combobox + chọn item đầu tiên
        void Combo(By by)
        {
            var b = w.Until(ExpectedConditions.ElementToBeClickable(by));
            js.ExecuteScript("arguments[0].scrollIntoView({block:'center'});", b);
            b.Click();
            ac.SendKeys(Keys.ArrowDown).SendKeys(Keys.Enter).Perform();
        }
        // 2. Chọn cửa hàng
        Combo(By.XPath("//label[contains(.,'Cửa hàng')]/following::button[@role='combobox'][1]"));
        // 3. Optional: ghi chú + giờ nhận
        try
        {
            w.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='orderNote']")))
             .SendKeys("Lấy tại cửa hàng chi nhánh đầu tiên, cảm ơn ạ!");

            Combo(By.XPath("//label[contains(normalize-space(),'Giờ nhận')]/following::button[@role='combobox'][1]"));
        }
        catch { }
        // 4. Đặt hàng
        var submit = w.Until(ExpectedConditions.ElementToBeClickable(
            By.XPath("//button[@type='submit' and contains(.,'Đặt hàng')]")));
        js.ExecuteScript("arguments[0].scrollIntoView({block:'center'});", submit);
        submit.Click();

        Console.WriteLine("Đã bấm 'Đặt hàng'.");
    }
    static void TryClosePopups(IWebDriver driver)
    {
        try
        {
            var candidates = new By[] {
                By.XPath("//button[contains(.,'Đồng ý') or contains(.,'Chấp nhận')]"),
                By.CssSelector("button.cookie-accept, .cookie-accept, .btn-accept-cookie"),
                By.CssSelector("button[aria-label='Close'], .modal-close, .close, .btn-close"),
                By.XPath("//*[text()='×' or text()='✕' or text()='Đóng']")
            };
            foreach (var by in candidates)
            {
                var els = driver.FindElements(by);
                foreach (var el in els)
                {
                    try { driver.ScrollIntoViewAndClick(el); } catch { }
                }
            }
        }
        catch { /* bỏ qua */ }
    }

}
