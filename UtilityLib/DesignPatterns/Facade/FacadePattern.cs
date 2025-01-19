using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DesignPatterns.Facade
{
    using System;

    namespace FacadeECommerceExample
    {
        // 在庫管理サブシステム
        public class Inventory
        {
            public bool CheckStock(string productId)
            {
                Console.WriteLine($"Inventory: Checking stock for product {productId}.");
                // 仮に在庫があるとする
                return true;
            }
        }

        // 支払いサブシステム
        public class Payment
        {
            public bool ProcessPayment(string paymentDetails)
            {
                Console.WriteLine($"Payment: Processing payment with details: {paymentDetails}.");
                // 仮に支払い成功とする
                return true;
            }
        }

        // 配送サブシステム
        public class Shipping
        {
            public void ArrangeShipping(string productId, string address)
            {
                Console.WriteLine($"Shipping: Arranging shipping for product {productId} to address {address}.");
            }
        }

        // Facadeクラス
        public class OrderFacade
        {
            private readonly Inventory _inventory;
            private readonly Payment _payment;
            private readonly Shipping _shipping;

            public OrderFacade()
            {
                _inventory = new Inventory();
                _payment = new Payment();
                _shipping = new Shipping();
            }

            public void PlaceOrder(string productId, string paymentDetails, string address)
            {
                Console.WriteLine("OrderFacade: Starting order process...");

                // 在庫を確認
                if (!_inventory.CheckStock(productId))
                {
                    Console.WriteLine("OrderFacade: Order failed - Product is out of stock.");
                    return;
                }

                // 支払い処理
                if (!_payment.ProcessPayment(paymentDetails))
                {
                    Console.WriteLine("OrderFacade: Order failed - Payment could not be processed.");
                    return;
                }

                // 配送手配
                _shipping.ArrangeShipping(productId, address);

                Console.WriteLine("OrderFacade: Order completed successfully!");
            }
        }

        // クライアントコード
        class Program
        {
            static void Main(string[] args)
            {
                // Facadeインスタンスを作成
                var orderFacade = new OrderFacade();

                // クライアントは簡単なインターフェイスを使って注文を実行
                string productId = "12345";
                string paymentDetails = "CreditCard 9876-5432-1098-7654";
                string address = "123 Main Street, Tokyo";

                orderFacade.PlaceOrder(productId, paymentDetails, address);
            }
        }
    }

}
