using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace TwitterClientSample {
    public class App : Application {
        public App() {
            MainPage = new MyPage();
        }

        
        
        private class MyPage : ContentPage {

            //private readonly ObservableCollection<string> _tweets = new ObservableCollection<string>();
            private readonly ObservableCollection<Tweet> _tweets = new ObservableCollection<Tweet>();

            public MyPage() {
                var entry = new Entry {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Text = "Xamarin"

                };

                var button = new Button {
                    WidthRequest = 80,
                    Text = "Search"
                };
                button.Clicked += (s, a) => {
                    Refresh(entry.Text);
                };

                var listView = new ListView {
                    ItemsSource = _tweets, // データソースの指定

                    ItemTemplate = new DataTemplate(typeof(MyCell)), //セルの指定
                    HasUnevenRows = true, //行の高さを可変とする
                };
                Content = new StackLayout {
                    Padding = new Thickness(0, Device.OnPlatform(20, 0, 0), 0, 0), // iOSのみ上余白
                    VerticalOptions = LayoutOptions.Fill,
                    Children = {
                        new StackLayout {
                            Padding = 5,
                            BackgroundColor = Color.Teal,
                            Orientation = StackOrientation.Horizontal, // 横に配置する
                            Children = {
                                entry,
                                button
                            }
                        },
                        listView
                    }
                };

            }

            async void Refresh(string keyword) {

                const string ApiKey = "API_KEY";
                const string ApiSecret = "API_SECRET";
                const string AccessToke = "ACCESS_TOKEN";
                const string AccessTokeSecret = "ACCESS_TOKEN_SECRET";


                var tokens = CoreTweet.Tokens.Create(ApiKey, ApiSecret, AccessToke, AccessTokeSecret);
                var result = await tokens.Search.TweetsAsync(count => 100, q => keyword);
                _tweets.Clear();
                //foreach (var tweet in result) {
                //    _tweets.Add(tweet.Text);
                //}

                foreach (var tweet in result) {
                    _tweets.Add(new Tweet {
                        Text = tweet.Text,
                        Name = tweet.User.Name,
                        ScreenName = tweet.User.ScreenName,
                        CreatedAt = tweet.CreatedAt.ToString("f"),
                        Icon = tweet.User.ProfileImageUrl.ToString()
                    });
                }
            }
            //１つのTweetを表現するクラス
            internal class Tweet {
                public string Name { get; set; } //表示名
                public string Text { get; set; } //メッセージ
                public string ScreenName { get; set; } //アカウント名
                public string CreatedAt { get; set; } //作成日時
                public string Icon { get; set; } //アイコン
            }

            //セル用のテンプレート
            private class MyCell : ViewCell {
                public MyCell() {

                    //アイコン
                    var icon = new Image();
                    icon.WidthRequest = icon.HeightRequest = 50; //アイコンのサイズ
                    icon.VerticalOptions = LayoutOptions.Start; //アイコンを行の上に詰めて表示
                    icon.SetBinding(Image.SourceProperty, "Icon");

                    //名前
                    var name = new Label {
                        FontSize = 12
                        //Font = Font.SystemFontOfSize(12)
                    };
                    name.SetBinding(Label.TextProperty, "Name");

                    //アカウント名
                    var screenName = new Label {
                        Font = Font.SystemFontOfSize(12)
                    };
                    screenName.SetBinding(Label.TextProperty, "ScreenName");

                    //作成日時
                    var createAt = new Label {
                        FontSize = 8,
                        //Font = Font.SystemFontOfSize(8), 
                        TextColor = Color.Gray
                    };
                    createAt.SetBinding(Label.TextProperty, "CreatedAt");

                    //メッセージ本文
                    var text = new Label {
                        FontSize = 10,
                        //Font = Font.SystemFontOfSize(10)
                    };
                    text.SetBinding(Label.TextProperty, "Text");

                    //名前行
                    var layoutName = new StackLayout {
                        Orientation = StackOrientation.Horizontal, //横に並べる
                        Children = {
                            name,
                            screenName
                        } //名前とアカウント名を横に並べる
                    };

                    //サブレイアウト
                    var layoutSub = new StackLayout {
                        Spacing = 0, //スペースなし
                        Children = {
                            layoutName,
                            createAt,
                            text
                        } //名前行、作成日時、メッセージを縦に並べる
                    };

                    View = new StackLayout {
                        Padding = new Thickness(5),
                        Orientation = StackOrientation.Horizontal, //横に並べる
                        Children = {
                            icon,
                            layoutSub
                        } //アイコンとサブレイアウトを横に並べる
                    };
                }
                //テキストの長さに応じて行の高さを増やす
                protected override void OnBindingContextChanged() {
                    base.OnBindingContextChanged();

                    //メッセージ
                    var text = ((Tweet)BindingContext).Text;
                    //メッセージを改行で区切って、各行の最大文字数を27として行数を計算する（27文字は、日本を基準にしました）
                    var row = text.Split('\n').Select(l => l.Length / 27).Select(c => c + 1).Sum();
                    Height = 12 + 8 + row * 10 + 20;//名前行、作成日時行、メッセージ行、パディングの合計値
                    if (Height < 60) {
                        Height = 60;//列の高さは、最低でも60とする
                    }
                }

            }

        }

    }
}