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
            // HttpClient���쐬
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // GET���N�G�X�g�𑗐M
                    HttpResponseMessage response = await client.GetAsync(url);

                    // �X�e�[�^�X�R�[�h���m�F
                    if (response.IsSuccessStatusCode)
                    {
                        // ���X�|���X�{�f�B���擾
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
                    // �G���[�n���h�����O
                    Console.WriteLine($"Request error: {e.Message}");
                }
            }

            return "���擾";
        }
    }
}
