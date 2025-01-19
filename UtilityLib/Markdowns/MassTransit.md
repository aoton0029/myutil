https://goatreview.com/masstransit-inmemory-mediator-basics/

MassTransitは、.NET向けのオープンソースの分散アプリケーションフレームワークであり、サービス間の通信を容易にし、スケーラブルで堅牢なアプリケーションの構築を支援します。その中でも、メディエーター（Mediator）パターンの実装は、コンポーネント間の通信を簡素化し、疎結合を維持するための重要な機能です。

**メディエーター（Mediator）パターンとは？**

オブジェクト指向プログラミングにおけるメディエーター（Mediator）パターンは、オブジェクト間の相互作用をカプセル化し、直接的な依存関係を減らす設計パターンです。MassTransitはこのパターンを活用し、プロセス内メッセージングを容易にし、アプリケーション設計の改善を図っています。

**IMediatorを使用したメッセージの送信**

MassTransitの`IMediator`を使用すると、同一プロセス内でメッセージを送信し、それを消費することができます。これは、`Send`や`Publish`メソッドがプロセス間通信に使用されるのとは異なり、プロセス内通信に特化しています。以下に`IMediator`を使用したメッセージ送信の例を示します。

```csharp
public static async Task<IResult> SubmitGoat(Guid id, IMediator mediator, CancellationToken cancellationToken)
{
    var response = await mediator.SendRequest<GoatSubmitted, GoatAccepted>(new GoatSubmitted(id), cancellationToken);
    // レスポンスの処理、例: return Results.Ok(response);
}
```

`SendRequest`メソッドは、リクエストを送信し、レスポンスを待機します。この際、送信するメッセージの型と受信するメッセージの型を指定します。重要なのは、コンシューマーが返す型がメディエーターで期待される型と一致している必要がある点です。一致しない場合、MassTransitはリンクを確立できず、エラーを引き起こします。

**IConsumerを使用したメッセージの処理**

`IConsumer`インターフェースは、特定の型のメッセージを消費するクラスを表します。本質的に、`IConsumer`はメッセージハンドラーであり、メッセージバスからメッセージを受信し、それを処理します。以下に`IConsumer`の実装例を示します。

```csharp
public class GoatSubmittedConsumer : IConsumer<GoatSubmitted>
{
    public async Task Consume(ConsumeContext<GoatSubmitted> context)
    {
        var goat = context.Message;
        // メッセージの処理ロジックをここに追加
        await context.RespondAsync<GoatAccepted>(new GoatAccepted());
    }
}
```

`ConsumeContext<T>`は、コンシューマーの`Consume`メソッド内で現在のメッセージを処理するための多くの有用な情報とユーティリティを提供します。主なものとして：

1. **Message**: 送信されたメッセージのプロパティにアクセスします。型は`T`です。通常、これはリクエストを処理するために必要な情報を持つ`record`です。

2. **CancellationToken**: 非同期関数全体で単一のトークンを転送する際に非常に有用です。これはメソッドだけでなく、メモリ内のメッセージ全体の管理に属します。特にEntity Framework Coreを使用している場合、非同期呼び出しでこのトークンを転送するのがデフォルトです。

3. **RespondAsync**: メディエーター パターンでは、送信者にメッセージを返します。これは、このパターンで必要とされる正確な動作です。リクエストの結果は、このメソッドを介して別の`record`として送信されます。

全体的に見て、`ConsumeContext<T>`は、メッセージの送受信方法の制御、エラー処理、リトライポリシーの制御、長時間実行される会話の管理など、コンシューマーでのメッセージ処理に関する幅広い機能を提供します。

**ServiceProviderへのコンシューマーの登録**

コンシューマーを使用するには、MassTransitが提供する`AddMediator`拡張メソッドを介して、依存性注入に登録します。以下にその例を示します。

```csharp
services.AddMediator(x =>
{
    // 個別にコンシューマーを登録
    x.AddConsumer<GoatSubmittedConsumer>();

    // 特定の名前空間内のすべてのコンシューマーを登録
    x.AddConsumersFromNamespaceContaining(typeof(GoatSubmittedConsumer));

    // 特定のアセンブリ内のすべてのコンシューマーを登録
    x.AddConsumers(typeof(GoatSubmittedConsumer).Assembly);
});
```

このようにして、`IMediator`の実装が依存性注入コンテナに登録され、アプリケーション内で利用可能になります。

**まとめ**

MassTransitのメディエーター パターンは、.NETアプリケーション内でのプロセス内メッセージングを簡素化し、コンポーネント間の疎結合を促進します。`IMediator`と`IConsumer`の組み合わせにより、開発 