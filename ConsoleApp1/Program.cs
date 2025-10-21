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
        By addToCartBy = By.XPath("//button[(contains(.,'Thêm vào giỏ') or contains(.,'Thêm vào giỏ hàng') or @name='add-to-cart' or @data-action='add-to-cart')]");
        var addToCartBtn = driver.SafeClickable(addToCartBy, 12);
        driver.ScrollIntoViewAndClick(addToCartBtn);

        // Mở giỏ/checkout để xác nhận
        driver.Navigate().GoToUrl("https://www.savor.vn/checkout");
        var pageText = driver.PageSource;
        Console.WriteLine(pageText.Contains("Giỏ hàng trống")
            ? "Giỏ hiện trống (có thể cần thử sản phẩm khác / selector khác)."
            : "Đã thêm sản phẩm và mở trang checkout.");
    }

    // ============================
    // HÀM 2: ĐẶT 1 CHIẾC BÁNH HỎA TỐC
    // ============================
    static void DatHang(IWebDriver driver)
    {
        // Vào thẳng trang checkout
        driver.Navigate().GoToUrl("https://www.savor.vn/checkout");
        TryClosePopups(driver);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

        // 1) Điền thông tin người đặt
        var hoTenDat = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='orderCustomer.name']")));
        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", hoTenDat);
        hoTenDat.Clear(); hoTenDat.SendKeys("Nguyen Van A");

        var sdtDat = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='orderCustomer.cellphone']")));
        sdtDat.Clear(); sdtDat.SendKeys("0912345678");

        // 2) Tick “Giống người đặt hàng”
        var giongNguoiDat = wait.Until(ExpectedConditions.ElementToBeClickable(
            By.XPath("//label[normalize-space()='Giống người đặt hàng']/ancestor::div[contains(@class,'leading-none')]/preceding-sibling::button[@role='checkbox']")));
        driver.ScrollIntoViewAndClick(giongNguoiDat);

        // 3) Tick “Lấy tại cửa hàng”
        var layTaiCuaHang = wait.Until(ExpectedConditions.ElementToBeClickable(
            By.XPath("//label[normalize-space()='Lấy tại cửa hàng']/ancestor::div[contains(@class,'leading-none')]/preceding-sibling::button[@role='checkbox']")));
        driver.ScrollIntoViewAndClick(layTaiCuaHang);

        // 4) Mở combobox “Cửa hàng”
        var chonCuaHangBtn = wait.Until(ExpectedConditions.ElementToBeClickable(
            By.XPath("//label[contains(.,'Cửa hàng')]/following::button[@role='combobox'][1]")));
        driver.ScrollIntoViewAndClick(chonCuaHangBtn);

        // 5) Khi danh sách mở ra, chọn cơ sở đầu tiên (ArrowDown + Enter)
        var actions = new OpenQA.Selenium.Interactions.Actions(driver);
        actions.SendKeys(Keys.ArrowDown).SendKeys(Keys.Enter).Perform();

        // 6) (Tùy chọn) Ghi chú đơn hàng
        try
        {
            var ghiChu = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='orderNote']")));
            ghiChu.Clear(); ghiChu.SendKeys("Lấy tại cửa hàng chi nhánh đầu tiên, cảm ơn ạ!");
        }
        catch { /* optional */ }

        // 7) (Tuỳ chọn) Chọn “Giờ nhận” (nếu form yêu cầu)
        try
        {
            var gioNhanBtn = wait.Until(ExpectedConditions.ElementToBeClickable(
                By.XPath("//label[contains(normalize-space(),'Giờ nhận')]/following::button[@role='combobox'][1]")));
            driver.ScrollIntoViewAndClick(gioNhanBtn);
            actions.SendKeys(Keys.ArrowDown).SendKeys(Keys.Enter).Perform();
        }
        catch { /* bỏ qua nếu không bắt buộc */ }

        // 8) Bấm nút “Đặt hàng”
        var datHangBtn = wait.Until(ExpectedConditions.ElementToBeClickable(
            By.XPath("//button[@type='submit' and contains(.,'Đặt hàng')]")));
        driver.ScrollIntoViewAndClick(datHangBtn);

        Console.WriteLine("Đã bấm 'Đặt hàng' với hình thức nhận tại cửa hàng.");
    }






    // Đóng popup/cookie/banner nếu có
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
