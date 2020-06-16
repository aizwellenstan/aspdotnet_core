# ASP.NET Core WebAPIモデルバインディングの認証メッセージをカスタマイズする

ASP.NET Core WebAPIを呼び出すためのHTTP Bodyとして複雑なJSONデータを使用する場合、ASP.NET CoreはJSONデータを対応するモデルバインディングとして使用します。モデルバインディングが失敗すると、ASP.NET Core WebAPIにはデフォルト設定があります。エラーメッセージは呼び出し元に返され、ASP.NET Coreアーキテクチャでは、変更のための柔軟性が維持されますASP.NET Core WebAPIモデルバインディングの検証メッセージをカスタマイズする方法を紹介します。

## モデルバインディングエラーメッセージの変更

![デフォルトのASP.NET Coreモデルバインディングエラーに応答するメッセージ](https://i.imgur.com/gFCxaGk.png)

デフォルトでは、ASP.NET Coreモデルが正しくバインドされていない場合の応答メッセージが上に示されています。最も重大なエラーメッセージは `"The value 'InvalidValue' is not valid."` 変更する場合この文では、 `ConfigureServices（）`でMVCオプションを調整できます。次のコードを参照してください：

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc()
        .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
        .AddMvcOptions(options =>
        {
            options.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor((x) => $"'The value{x}' is not valid.");
        });
}
```

ここでは、 `AddMvcOptions（）`を使用して、MVCで使用されるオプションを変更します。デフォルトの `ModelBindingMessageProvider`モデルバインディング情報プロバイダーには、次の11のメッセージがあります（または[公式ドキュメント](https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.aspnetcore.mvc.modelbinding.metadata.modelbindingmessageprovider?view=aspnetcore-3.0&WT.mc_id=DT-MVP-5003022)）：

| プロパティ                                          | デフォルトのメッセージテンプレート                                                  |
| -------------------------------------------- | ------------------------------------------------------------- |
| `MissingBindRequiredValueAccessor`           | A value for the '{0}' parameter or property was not provided. |
| `MissingKeyOrValueAccessor`                  | A value is required.                                          |
| `MissingRequestBodyRequiredValueAccessor`    | A non-empty request body is required.                         |
| `ValueMustNotBeNullAccessor`                 | The value '{0}' is invalid.                                   |
| `AttemptedValueIsInvalidAccessor`            | The value '{0}' is not valid for {1}.                         |
| `NonPropertyAttemptedValueIsInvalidAccessor` | The value '{0}' is not valid.                                 |
| `UnknownValueIsInvalidAccessor`              | The supplied value is invalid for {0}.                        |
| `NonPropertyUnknownValueIsInvalidAccessor`   | The supplied value is invalid.                                |
| `ValueIsInvalidAccessor`                     | The value '{0}' is invalid.                                   |
| `ValueMustBeANumberAccessor`                 | The field {0} must be a number.                               |
| `NonPropertyValueMustBeANumberAccessor`      | The field must be a number.                                   |


ただし、プロパティを直接変更することはできません。アクセサーを使用して変更する必要があります。

その場合、各メッセージの変更方法はわずかに異なりますが、変更のためにいくつかのパラメータを受け取る必要があることに注意してください。

この情報は、GitHub [aspnet/Mvc](https://github.com/aspnet/Mvc) の [DefaultModelBindingMessageProvider.cs](https://github.com/aspnet/Mvc/blob/master/src/Microsoft.AspNetCore.Mvc.Core/ModelBinding/Metadata/DefaultModelBindingMessageProvider.cs)，モデルバインディングメッセージのすべての属性があり、リソースは次の場所にあります [Microsoft.AspNetCore.Mvc.Core/Resources.resx](https://github.com/aspnet/Mvc/blob/master/src/Microsoft.AspNetCore.Mvc.Core/Resources.resx)既定のメッセージテンプレートを見つけます。

## モデルバインディングエラーメッセージオブジェクト全体を変更する

モデル全体が誤ってバインドされたときに応答メッセージを変更する場合は、 `ConfigureServices（）`でMVCオプションを調整することもできます。次のコードを参照してください：

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc()
        .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = actionContext => new BadRequestObjectResult(new { Message = "Model binding occurs problem." });
        });
}
```
このコードを非常に単純なものに変更しました。モデルのバインディングが間違っている場合、`Model binding occurs problem.`ように`Message`属性値を含むオブジェクトを直接返します。

![デフォルトのASP.NET Coreモデルバインディングエラーに応答するメッセージ](https://i.imgur.com/8wsvFqz.png)

MVCでモデルバインディングレスポンスを生成するために使用されるファクトリオブジェクト `InvalidModelStateResponseFactory`を変更するための` ConfigureApiBehaviorOptions（） `により、モデルバインディングエラーが発生したときに実行するアクションと、送信するメッセージの種類をカスタマイズできます発信者。

上記のコードを非常にシンプルに変更し、 `BadRequestObjectResult`を使用してHTTP 400ステータスレスポンスを生成し、シンプルなメッセージオブジェクトを含めるだけで、このセクションを個別に抽出して処理できます。モデルバインディングエラーに対応するログやチャネルへの通知などのカスタムロジックを作成し、必要な応答メッセージをカスタマイズします。

----------

参照：

* [ASP.NET Core Model Binding Error Messages Localization](https://stackoverflow.com/questions/40828570/asp-net-core-model-binding-error-messages-localization/41669552)
* [Customizing a Model Validation Response which results in an HTTP 400 Error Code](https://www.c-sharpcorner.com/blogs/customizing-model-validation-response-resulting-as-http-400-in-net-core)
* [Exploring the ApiControllerAttribute and its features for ASP.NET Core MVC 2.1](https://www.strathweb.com/2018/02/exploring-the-apicontrollerattribute-and-its-features-for-asp-net-core-mvc-2-1/)
