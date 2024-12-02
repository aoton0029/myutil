using System.Text.Json;
using System.Text.RegularExpressions;

namespace HttpClientSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void btnGet_Click(object sender, EventArgs e)
        {
            string s = await post(txtUrl.Text);
            if (InvokeRequired)
            {
                Invoke(new Action(() => {
                    textBox1.Text = Regex.Unescape(s);
                    BooklogModel m = JsonSerializer.Deserialize<BooklogModel>(Regex.Unescape(s));
                }));
            }
            else
            {
                textBox1.Text = Regex.Unescape(s);
            }
        }

        private async Task<string> post(string url)
        {
            // HttpClientを作成
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // GETリクエストを送信
                    HttpResponseMessage response = await client.GetAsync(url);

                    // ステータスコードを確認
                    if (response.IsSuccessStatusCode)
                    {
                        // レスポンスボディを取得
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Response Body:");
                        Console.WriteLine(responseBody);
                        return responseBody;
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                    }
                }
                catch (HttpRequestException e)
                {
                    // エラーハンドリング
                    Console.WriteLine($"Request error: {e.Message}");
                }
            }

            return "未取得";
        }
    }
}
