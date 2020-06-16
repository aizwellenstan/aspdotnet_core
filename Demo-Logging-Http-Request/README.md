# ASP.NET Core WebサイトのすべてのHTTP要求情報を収集します

Web APIの開発中にこの状況が発生する可能性がありますWebサイトに対して行われたすべてのHTTPリクエストを収集したい場合呼び出し元APIのURL、HTTPメソッド、さらにはHTTPリクエストのコンテンツ（リクエストボディ）から、 ASP.NET Coreのプログラムアーキテクチャでは、プロジェクト構造の調停プロセスでHTTP情報をインターセプトし、必要な処理を実行できます。

![メディエーションプログラムによるHTTPリクエストの処理フロー](https://i.imgur.com/84xhdDE.png)

上の図は、ASP.NET Core中間プログラムアーキテクチャの単純な表現です。今回の目標は、ASP.NET Core中間プログラムにカスタムロギングミドルウェアを追加して、すべてのHTTPリクエストがこのアプリケーションに入ることです。私たちによって傍受され、記録されます。

## ロギングを開始

まず、ASP.NET Coreのビルトイン [ロギングロガー]，ただし `Program.cs` ファイルでロギングメカニズムを次のように設定します。

```csharp
public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        // ロギングメカニズムに参加する
        .ConfigureLogging((logging) =>
        {
            logging.AddDebug();
            logging.AddConsole();
        })
        .UseStartup<Startup>();
```

デフォルトでは、この関数は `appsettings.json` (または開発中に使用される `appsettings.Development.json`) の設定を使用します。このレコーダーがトレースレベル情報を記録するには、これを変更する必要がありますファイル Logging のデフォルトの LogLevel は、次のように `Trace` です。

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "System": "Information",
      "Microsoft": "Information"
    }
  }
}
```

## カスタムロギングミドルウェア

ここにポイントがあります。それから `LoggingMiddleware.cs` 中間プログラムを追加します。これは HTTP Request リクエストの処理全体を実装して記録するためのキーになります。


要求がこのメディエーションプログラムに入ると、要求は処理のためにロガーにコピーされ、同じ要求が渡されます。

![ロギングメディエーションプログラムのアクション](https://i.imgur.com/NLMxwL9.png)

コピーを作成する理由これは、ASP.NET CoreのHttpContextでは、HTTPの `Request.Body`プロパティが` Stream`型であり、このプロパティは1回しか読み取れないためです。保存されない場合、後続の中間プログラムは取得できません。 HTTPリクエストを完了し、アプリケーション例外を引き起こします。

したがって、ここでの処理には特別な注意が必要な場所がいくつかあります。最初に次のコードを見てください。

```csharp
public async Task Invoke(HttpContext context)
{
    // HTTPリクエストが複数回読み取れることを確保します
    context.Request.EnableBuffering();

    // HTTPリクエスト本文のコンテンツを読み取ります
    // 注意！ leaveOpenプロパティをtrueに設定してStreamReaderを閉じると、HTTP要求のストリームは閉じられません。
    using (var bodyReader = new StreamReader(stream: context.Request.Body,
                                              encoding: Encoding.UTF8,
                                              detectEncodingFromByteOrderMarks: false,
                                              bufferSize: 1024,
                                              leaveOpen: true))
    {
        var body = await bodyReader.ReadToEndAsync();
        var log = $"{context.Request.Path}, {context.Request.Method}, {body}";
        _logger.LogTrace(log);
    }

     // HTTPリクエストのストリーム開始位置をゼロにします
    context.Request.Body.Position = 0;

    await _next.Invoke(context);
}
```

最初のステップでは、HTTPリクエストを複数回読み取れるようにするために、 `context.Request.EnableBuffering（）;`を実行してキャッシュメカニズムを有効にする必要があります。

2番目のステップでは、HTTPリクエストにアクセスするStreamReaderを作成するとき、StreamReaderがStreamを閉じた後にStreamReader（ `Request.Body`）のソースが閉じないように、StreamReaderコンストラクターで` leaveOpen: true`を設定する必要があります。ポイントが重要です！そうしないと、アプリケーションに次のようなエラーメッセージが表示されます。

```json
{
    "errors": {
        "": [
            "A non-empty request body is required."
        ]
    },
    "title": "One or more validation errors occurred.",
    "status": 400,
    "traceId": "8000001e-0001-ff00-b63f-84710c7967bb"
}
```

`A non-empty request body is required` とは、HTTPリクエストを処理する後続の仲介者が空の「Request.Body」を受け入れることができないことを伝えるため、エラーが発生します。

3番目のステップは、「StreamReader.ReadToEndAsync（）」を使用してデータを読み取ることですデータを読み取った後、あなたは何でもできますたとえば、上記のシナリオで述べたように、すべてのHTTPリクエストを保存および記録します。

4番目のステップは、「context.Request.Body.Position = 0;」を使用して元のストリームをゼロにすることです。

## カスタムロギングミドルウェアメディエーションを有効にする

カスタムログミドルウェアメディエーションプロセスを適切に有効にするために、サンプルコードでLoggingMiddlewareExtensions拡張メソッドが作成されます。

```csharp
public static class LoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LoggingMiddleware>();
    }
}
```

この拡張メソッドにより、直感的なメソッド `app.UseLoggingMiddleware（）;`を使用することにより、 `Startup.cs`ファイルの` Configure（） `でロギングミドルウェアミドルウェアを有効にでき、簡単に起動できます。

参照：

* [getting the request body inside HttpContext from a Middleware in asp.net core 2.0](https://stackoverflow.com/questions/47624938/getting-the-request-body-inside-httpcontext-from-a-middleware-in-asp-net-core-2)
* [Using Middleware in ASP.NET Core to Log Requests and Responses](https://exceptionnotfound.net/using-middleware-to-log-requests-and-responses-in-asp-net-core/)
* [Logging the Body of HTTP Request and Response in ASP .NET Core](http://www.palador.com/2017/05/24/logging-the-body-of-http-request-and-response-in-asp-net-core/)
* [Re-reading ASP.Net Core request bodies with EnableBuffering()](https://devblogs.microsoft.com/aspnet/re-reading-asp-net-core-request-bodies-with-enablebuffering/)
