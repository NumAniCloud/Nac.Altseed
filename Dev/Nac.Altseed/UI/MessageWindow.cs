﻿using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using asd;
using Nac.Altseed.Input;
using Nac.Altseed.Linq;
using Nac.Altseed.ObjectSystem;
using System.Reactive.Disposables;

namespace Nac.Altseed.UI
{
	public class MessageWindow : ReactiveTextureObject2D
	{
		private Subject<Unit> onRead_ = new Subject<Unit>();
        private Func<bool> isReadKeyPushed;
		private bool isSkipping;

		public TextObject2D TextObject { get; private set; }
		public TextureObject2D WaitIndicator { get; private set; }
		public float TextSpeed { get; set; }
		public IObservable<Unit> OnRead => onRead_;

		public MessageWindow()
		{
			TextObject = new TextObject2D()
			{
				Text = "",
				DrawingPriority = 1,
				TextureFilterType = TextureFilterType.Linear
			};
			WaitIndicator = new TextureObject2D()
			{
				IsDrawn = false,
				DrawingPriority = 1,
			};
			AddDrawnChild(TextObject,
				ChildManagementMode.RegistrationToLayer | ChildManagementMode.Disposal,
				ChildTransformingMode.All,
				ChildDrawingMode.DrawingPriority);
			TextObject.AddDrawnChild(WaitIndicator,
				ChildManagementMode.RegistrationToLayer | ChildManagementMode.Disposal,
				ChildTransformingMode.All,
				ChildDrawingMode.DrawingPriority);
			TextSpeed = 1;
			isSkipping = false;
		}

        public void SetReadControl<TAbstractKey>(Controller<TAbstractKey> controller, TAbstractKey readKey)
        {
            isReadKeyPushed = () => controller.GetState(readKey) == InputState.Push;
        }

		public void SetSkipControl<TAbstractKey>(Controller<TAbstractKey> controller, TAbstractKey skipKey, float requiredHoldTime)
		{
			var disposable = new CompositeDisposable();

			var push = OnUpdateEvent.Where(x => controller.GetState(skipKey) == InputState.Push);
			var release = OnUpdateEvent.Where(x => controller.GetState(skipKey) == InputState.Release);

			// 1秒間押し続けるとスキップ起動
			var inner = OnUpdateEvent.Select<float, Unit?>(x => Unit.Default)
				.TakeUntil(release)
				.SkipUntil(Observable.Timer(TimeSpan.FromSeconds(requiredHoldTime)))
				.FirstOrDefaultAsync();
			push.SelectMany(inner)
				.Where(x => x != null)
				.Subscribe(x => isSkipping = true)
				.AddTo(disposable);
			// ボタンを離すとスキップ終了
			release.Subscribe(x => isSkipping = false)
				.AddTo(disposable);

			OnDisposeEvent.Subscribe(x => disposable.Dispose());
		}

		public Task TalkMessageAsync(string[] message)
		{
			return TalkMessageAsync(message, new CancellationToken());
		}

		public Task TalkMessageWithoutReadAsync(string message)
		{
			return TalkMessageWithoutReadAsync(message, new CancellationToken());
		}

		/// <summary>
		/// メッセージ文字列の配列を表示します。読み進める操作が入力されるたびに次の要素を表示します。
		/// </summary>
		/// <param name="message">表示するメッセージの配列。</param>
		/// <param name="ct">キャンセル トークン。</param>
		/// <returns></returns>
		public async Task TalkMessageAsync(string[] message, CancellationToken ct)
		{
			if(!IsAlive)
			{
				return;
			}
			OutputWarningOfTextObject();
			await OnUpdateEvent.SelectCorourine(FlowToShowText(message, true)).ToTask(ct);
		}

		/// <summary>
		/// メッセージ文字列を表示します。
		/// </summary>
		/// <param name="message">表示するメッセージ。</param>
		/// <param name="ct">キャンセル トークン。</param>
		/// <returns></returns>
		public async Task TalkMessageWithoutReadAsync(string message, CancellationToken ct)
		{
			if(!IsAlive)
			{
				return;
			}
			OutputWarningOfTextObject();
			await OnUpdateEvent.SelectCorourine(FlowToShowText(new string[] { message }, false)).ToTask(ct);
		}

		/// <summary>
		/// 文字列をウィンドウに表示します。タイプライター風のアニメーションをしません。
		/// </summary>
		/// <param name="message">表示するメッセージ。</param>
		public void ShowMessage(string message)
		{
			if(!IsAlive)
			{
				return;
			}
			OutputWarningOfTextObject();
			TextObject.Text = message ?? "";
		}

        public void Clear()
		{
			if(!IsAlive)
			{
				return;
			}
			OutputWarningOfTextObject();
			TextObject.Text = "";
        }

		
		private IEnumerator<Unit> FlowToShowText(string[] text, bool readKeyIsNecessary)
		{
			foreach (var message in text)
			{
				float charCount = 0;
				while(charCount < message.Length)
				{
					charCount += TextSpeed;

					if(isReadKeyPushed?.Invoke() == true || isSkipping)
					{
						charCount = message.Length;
					}

					TextObject.Text = message.Substring(0, (int)charCount);
					yield return Unit.Default;
				}

				if(readKeyIsNecessary)
				{
					WaitIndicator.IsDrawn = true;
					while(isReadKeyPushed?.Invoke() != true && !isSkipping)
					{
						yield return Unit.Default;
					}
					onRead_.OnNext(Unit.Default);
					WaitIndicator.IsDrawn = false;
				}

				yield return Unit.Default;
			}
		}

		private void OutputWarningOfTextObject()
		{
			if(IsAlive && TextObject.Font == null)
			{
				Debug.WriteWarning(this, "TextObject.Font が null です。メッセージは表示されません。");
			}
		}
	}
}
