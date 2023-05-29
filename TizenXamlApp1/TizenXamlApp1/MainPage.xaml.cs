using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml;
using Newtonsoft.Json;



namespace TizenXamlApp1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    { 
        string[,] news = { 
            { "	강제동원 생존 피해자, 판결금 첫 수령", "김윤덕 의원 \"세계스카우트잼버리대회, 배수 문제 해결 시급\"", "태국, F-35 전투기 구매 불발될 듯…\"미국, 판매 불가 입장\"", "여아 합의 '전세사기특별법'에 피해자들 \"반쪽짜리\""} , 
            { "로또 1등 당첨에도 밀린 세금 안 냈다…'간 큰' 체납자의 최후", "팸텍, 코스닥 시장 상장…\"글로벌 경쟁력 강화\"", "‘깡통전세’ 100채로 149억 대출 받아 꿀꺽한 일당", "GH, 기존주택전세임대 입주자 수시모집"} , 
            { "tvN 음악 예능 '댄스가수 유랑단' 제작발표회", "'연진아 예능 봐야지, 이거 되게 신나!'", "누가 저쩌구", "아이고 일났다" }, 
            { "대한민국 프랑스 2-1 격파 'U-20 월드컵 최대 이변'", "KBL 역대 최다 보수 인상률 100%+ FA 11명", "'빈볼 논란' 벤치클리어링 부르는 보복구 ", "남자배구, 국가대표팀 명단 발표…정지석 복귀" } 
        };
        string[] newsCategory = { "정치", "경제", "연예", "스포츠" };
        private const string apiUrl = "https://api.openai.com/v1/chat/completions";
        private const string apiKey = "";
        private async void backgroundChange() // 배경화면 변경 함수
        {
            Image[] bg = { background1, background2, background3, background4 };
            int cnt = 0;
            while (true)
            {
                await Task.Delay(10000);
                await Task.WhenAll(
                    bg[cnt].FadeTo(0, 5000),
                    bg[(cnt + 1) % 4].FadeTo(1, 5000)
                );
                cnt = (cnt + 1) % bg.Length;
            }
        }

        private void AnalogClock(DateTime time) // 아날로그 시계 시간 변경 함수
        {
            aHour.RotateTo(time.Hour % 12 * 30 + time.Minute * 0.5 + time.Second * 0.5 / 60, 1);
            aMinute.RotateTo((time.Minute * 6 + time.Second * 0.1) % 360, 1);
            aSecond.RotateTo(time.Second * 6 % 360, 1);
        }
        private void DigitalClock(DateTime time) // 디지털 시계 시간 변경 함수
        {
            dHour.Text = string.Format("{0:00}", time.Hour);
            dMinute.Text = string.Format("{0:00}", time.Minute);
            if (dColon.Opacity == 1) dColon.Opacity = 0;
            else dColon.Opacity = 1;
        }

        private async void DatePannel() // 날짜 및 요일 패널 이동 함수
        {
            await Task.Delay(2000);
            double translatePosition = dDate.Width + dDate.X;
            while (true)
            {
                await dDate.TranslateTo(-translatePosition, 0, 3000);
                dDate.TranslationX = translatePosition;
                await dDate.TranslateTo(0, 0, 3000);
                await Task.Delay(5000);
            }
        }
        private void newsButtonClicked(object sender, EventArgs e) // 뉴스 카테고리 버튼 이벤트 함수
        {
            Button button = (Button)sender;
            newsGenre.Text = button.Text + " 속보";
        }
        private async void newsBanner() // 뉴스 배너 이동 함수
        {
            int genre = 0, newsidx = 0;
            string g = newsGenre.Text;
            await Task.Delay(2000);
            double translatePosition = headline.Width + headline.X;
            while (true)
            {
                if(g != newsGenre.Text)
                {
                    g = newsGenre.Text;
                    genre = Array.IndexOf(newsCategory, g.Replace(" 속보", ""));
                    newsidx = 0;
                }
                headline.Text = news[genre, newsidx];
                newsidx = (newsidx + 1) % 4;
                await headline.TranslateTo(-1890, 0, 5000);
                await Task.Delay(3000);
                await headline.TranslateTo(-translatePosition, 0, 5000);
                headline.TranslationX = translatePosition;
                await headline.TranslateTo(0, 0, 1000);
            }
        }
        
        private async void OnSendButtonClicked(object sender, EventArgs e) // Chat GPT 
        {
            string userInput = input.Text;
            var request = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = "You are a helpful assistant."
                    },
                    new
                    {
                        role = "user",
                        content = userInput + ". please answer simple."
                    }
                },
                max_tokens=100
            };
            string requestData = JsonConvert.SerializeObject(request);
            var content = new StringContent(requestData, Encoding.UTF8, "application/json");
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic apiResponse = JsonConvert.DeserializeObject(responseContent);

                    // API 응답 처리
                    output.Text = apiResponse.choices[0].message.content;
                }
                else
                {
                    output.Text = $"API request failed with status code {response.StatusCode}: {response.ReasonPhrase}";
                }
            }
        }

        public MainPage()
        {
            InitializeComponent();
            backgroundChange();
            DatePannel();
            newsBanner();

            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                DateTime currentTime = DateTime.Now;
                dDate.Text = currentTime.ToString("yy-MM-dd-ddd");
                AnalogClock(currentTime);
                DigitalClock(currentTime);
                return true;
            });
        }
    }
}

