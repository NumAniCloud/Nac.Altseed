﻿using System;
using System.Reactive;
using System.Reactive.Subjects;
using asd;
using Altseed.Reactive.Helper;
using System.Reactive.Disposables;

namespace Altseed.Reactive.Object
{
	/// <summary>
	/// 特定のタイミングでイベントを発行する2Dレイヤー。
	/// </summary>
	public class ReactiveLayer2D : Layer2D, INotifyUpdated, IDisposer
	{
		private Subject<Unit> onAddedEvent_ = new Subject<Unit>();
		private Subject<Unit> onRemovedEvent_ = new Subject<Unit>();
		private Subject<long> onUpdatingEvent_ = new Subject<long>();
		private Subject<long> onUpdatedEvent_ = new Subject<long>();
		private Subject<Unit> onDisposeEvent_ = new Subject<Unit>();
		private CompositeDisposable disposable = new CompositeDisposable();

		/// <summary>
		/// レイヤーに追加された時に通知するイベントを取得します。レイヤーが破棄されたとき完了します。
		/// </summary>
		public IObservable<Unit> OnAddedEvent => onAddedEvent_;
		/// <summary>
		/// レイヤーから削除された時に通知するイベントを取得します。レイヤーが破棄されたとき完了します。
		/// </summary>
		public IObservable<Unit> OnRemovedEvent => onRemovedEvent_;
		/// <summary>
		/// 更新される直前に発行されるイベント。破棄されたとき完了します。
		/// 値として何個目のイベントかのインデックスが流れます。
		/// </summary>
		public IObservable<long> OnUpdatingEvent => onUpdatingEvent_.Total();
		/// <summary>
		/// 更新された直後に発行されるイベント。破棄されたとき完了します。
		/// 値として何個目のイベントかのインデックスが流れます。
		/// </summary>
		public IObservable<long> OnUpdatedEvent => onUpdatedEvent_.Total();
		/// <summary>
		/// このレイヤーが破棄されたときに発行されるイベント。発行されると同時に完了します。
		/// </summary>
		public IObservable<Unit> OnDisposeEvent => onDisposeEvent_;

		protected override void OnAdded()
		{
			onAddedEvent_.OnNext(Unit.Default);
		}

		protected override void OnRemoved()
		{
			onRemovedEvent_.OnNext(Unit.Default);
		}

		protected override void OnUpdating()
		{
			onUpdatingEvent_.OnNext(1);
		}

		protected override void OnUpdated()
		{
			onUpdatedEvent_.OnNext(1);
		}

		protected override void OnDispose()
		{
			onDisposeEvent_.OnNext(Unit.Default);
			onAddedEvent_.OnCompleted();
			onRemovedEvent_.OnCompleted();
			onUpdatingEvent_.OnCompleted();
			onUpdatedEvent_.OnCompleted();
			onDisposeEvent_.OnCompleted();
			disposable.Dispose();
		}

		/// <summary>
		/// このオブジェクトが破棄されるときに一緒に破棄されるインスタンスを設定します
		/// </summary>
		/// <param name="resource">このオブジェクトが破棄されるときに一緒に破棄されるインスタンス。</param>
		public void AddDisposable(IDisposable resource)
		{
			disposable.Add(resource);
		}
	}
}
