using System;
using System.Threading;
using System.Threading.Tasks;


namespace Sc.Util.System
{
	/// <summary>
	/// Simple thread-safe <see cref="IDisposable"/> class that invokes an
	/// <see cref="Action"/> in <see cref="IDisposable.Dispose"/>. Will not invoke the
	/// delegate twice. The reurned implementation also implements
	/// <see cref="IDispose"/>--- which raises an event and provides
	/// <see cref="IRaiseDisposed.IsDisposed"/>.
	/// </summary>
	public abstract class DelegateDisposable
			: IDisposable
	{
		/// <summary>
		/// Action implementation.
		/// </summary>
		private class NoState
				: DelegateDisposable
		{
			/// <summary>
			/// Action implementation with a finalizer.
			/// </summary>
			internal sealed class Finalizing
					: NoState
			{
				/// <summary>
				/// Cnstructor.
				/// </summary>
				/// <param name="dispose">Not null.</param>
				internal Finalizing(Action<bool> dispose)
						: base(dispose) { }


				~Finalizing()
					=> Dispose(false);
			}


			private Action<bool> disposeDelegate;


			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="dispose">Required.</param>
			internal NoState(Action<bool> dispose)
				=> disposeDelegate = dispose ?? throw new ArgumentNullException(nameof(dispose));


			public sealed override bool IsDisposed
				=> disposeDelegate == null;

			protected sealed override void Dispose(bool isDisposing)
			{
				Interlocked.Exchange(ref disposeDelegate, null)
						?.Invoke(isDisposing);
				base.Dispose(isDisposing);
			}
		}


		/// <summary>
		/// State implementation.
		/// </summary>
		/// <typeparam name="TState">Your state type.</typeparam>
		private class WithState<TState>
				: DelegateDisposable<TState>
		{
			/// <summary>
			/// State implementation with a finalizer.
			/// </summary>
			internal sealed class Finalizing
					: WithState<TState>
			{
				/// <summary>
				/// Cnstructor.
				/// </summary>
				/// <param name="constructor">Required.</param>
				/// <param name="dispose">Not null.</param>
				internal Finalizing(Func<TState> constructor, Action<TState, bool> dispose)
						: base(constructor, dispose) { }


				~Finalizing()
					=> Dispose(false);
			}


			private Action<TState, bool> disposeDelegate;


			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="constructor">Required.</param>
			/// <param name="dispose">Required.</param>
			internal WithState(Func<TState> constructor, Action<TState, bool> dispose)
			{
				if (constructor == null)
					throw new ArgumentNullException(nameof(constructor));
				disposeDelegate = dispose ?? throw new ArgumentNullException(nameof(dispose));
				State = constructor();
			}


			public sealed override bool IsDisposed
				=> disposeDelegate == null;

			protected sealed override void Dispose(bool isDisposing)
			{
				Interlocked.Exchange(ref disposeDelegate, null)
						?.Invoke(State, isDisposing);
				State = default;
				base.Dispose(isDisposing);
			}
		}


		/// <summary>
		/// Static constructor method. Invokes your <see cref="Action"/> in
		/// <see cref="IDisposable.Dispose"/>. Will not invoke the delegate twice.
		/// </summary>
		/// <param name="dispose">Not null.</param>
		/// <returns>Not null.</returns>
		public static DelegateDisposable With(Action dispose)
			=> new NoState(_ => dispose());

		/// <summary>
		/// Static constructor method for an instance that implements a finalizer. Invokes your
		/// <see cref="Action"/> in <see cref="IDisposable.Dispose"/> or the finalizer. Will not
		/// invoke the delegate twice.
		/// </summary>
		/// <param name="dispose">Not null. The argument is true if invoked from Dispose;
		/// and false if invoked from the finalizer.</param>
		/// <returns>Not null.</returns>
		public static DelegateDisposable With(Action<bool> dispose)
			=> new NoState.Finalizing(dispose);


		/// <summary>
		/// Static constructor method. Creates a new <see cref="Task"/> that will complete when the
		/// returned object is <see cref="IDisposable.Dispose"/>. Also will cancel the task if you
		/// provide a token; and allows a Task result.
		/// </summary>
		/// <param name="task">The task that will complete when the result is disposed.</param>
		/// <param name="taskResult">Optional. Will set the result of the completed <c>task</c>.</param>
		/// <param name="cancellationToken">Optional token that can cancel the returned <c>task</c>.</param>
		/// <returns>Not null.</returns>
		public static DelegateDisposable When<T>(
				out Task<T> task,
				Func<T> taskResult = default,
				CancellationToken cancellationToken = default)
		{
			DelegateDisposable<(TaskCompletionSource<T> tcs, Func<T> taskResult, Action<object> tokenCallback)> result
					= DelegateDisposable.With
							<(TaskCompletionSource<T> tcs, Func<T> taskResult, Action<object> tokenCallback),
							Func<T>>(
							taskResultDelegate =>
							{
								TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>();
								if (!cancellationToken.CanBeCanceled)
									return (taskCompletionSource, taskResultDelegate, null);
								// ReSharper disable once ConvertToLocalFunction
								Action<object> tokenCallback = tcs => ((TaskCompletionSource<T>)tcs).TrySetCanceled();
								cancellationToken.Register(tokenCallback, taskCompletionSource);
								if (cancellationToken.IsCancellationRequested)
									taskCompletionSource.TrySetCanceled();
								return (taskCompletionSource, taskResultDelegate, tokenCallback);
							},
							taskResult ?? (() => default),
							tuple =>
							{
								if (!cancellationToken.CanBeCanceled
										|| !cancellationToken.IsCancellationRequested)
									tuple.tcs.TrySetResult(tuple.taskResult());
							});
			task = result.State.tcs.Task;
			return result;
		}


		/// <summary>
		/// Static constructor method. This generic class adds a <see cref="DelegateDisposable{T}.State"/>
		/// object that you can provide from a delegate when this object is constructed. Invokes your
		/// <see cref="Action"/> in <see cref="IDisposable.Dispose"/>. Will not invoke the delegate twice.
		/// </summary>
		/// <param name="constructor">Not null.</param>
		/// <param name="dispose">Not null.</param>
		/// <returns>Not null.</returns>
		public static DelegateDisposable<TState> With<TState>(Func<TState> constructor, Action<TState> dispose)
			=> new WithState<TState>(constructor, (state, _) => dispose(state));

		/// <summary>
		/// Static constructor method. This this generic class adds a <see cref="DelegateDisposable{T}.State"/>
		/// object that you can provide from a delegate when this object is constructed; and this method
		/// allows your constructor delegate to provide itself an argument. Invokes your <see cref="Action"/>
		/// in <see cref="IDisposable.Dispose"/>. Will not invoke the delegate twice.
		/// </summary>
		/// <param name="constructor">Not null.</param>
		/// <param name="constructorArg">An arbitrary argument for your own <c>constructor</c>.</param>
		/// <param name="dispose">Not null.</param>
		/// <returns>Not null.</returns>
		public static DelegateDisposable<TState> With<TState, TArg>(
				Func<TArg, TState> constructor,
				TArg constructorArg,
				Action<TState> dispose)
			=> new WithState<TState>(() => constructor(constructorArg), (state, _) => dispose(state));


		/// <summary>
		/// Static constructor method for an instance that implements a finalizer.This generic class adds a
		/// <see cref="DelegateDisposable{T}.State"/> object that you can provide from a delegate when this
		/// object is constructed. Invokes your <see cref="Action"/> in <see cref="IDisposable.Dispose"/>
		/// or the finalizer. Will not invoke the delegate twice.
		/// </summary>
		/// <param name="constructor">Not null.</param>
		/// <param name="dispose">Not null. The bool argument is true if invoked from Dispose;
		/// and false if invoked from the finalizer.</param>
		/// <returns>Not null.</returns>
		public static DelegateDisposable<TState> With<TState>(
				Func<TState> constructor,
				Action<TState, bool> dispose)
			=> new WithState<TState>.Finalizing(constructor, dispose);

		/// <summary>
		/// Static constructor method for an instance that implements a finalizer. This this generic class
		/// adds a <see cref="DelegateDisposable{T}.State"/> object that you can provide from a delegate when
		/// this object is constructed; and this method allows your constructor delegate to provide itself
		/// an argument. Invokes your <see cref="Action"/> in <see cref="IDisposable.Dispose"/> or the
		/// finalizer. Will not invoke the delegate twice.
		/// </summary>
		/// <param name="constructor">Not null.</param>
		/// <param name="constructorArg">An arbitrary argument for your own <c>constructor</c>.</param>
		/// <param name="dispose">Not null. The bool argument is true if invoked from Dispose;
		/// and false if invoked from the finalizer.</param>
		/// <returns>Not null.</returns>
		public static DelegateDisposable<TState> With<TState, TArg>(
				Func<TArg, TState> constructor,
				TArg constructorArg,
				Action<TState, bool> dispose)
			=> new WithState<TState>.Finalizing(() => constructor(constructorArg), dispose);


		public abstract bool IsDisposed { get; }

		public event EventHandler Disposed;

		/// <summary>
		/// Raises the event; only if the argument is true.
		/// </summary>
		/// <param name="isDisposing">True if invoked from Dispose; else a finalizer.</param>
		protected virtual void Dispose(bool isDisposing)
		{
			if (!isDisposing)
				return;
			Disposed?.Invoke(this, EventArgs.Empty);
			Disposed = null;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}
	}


	/// <summary>
	/// Simple thread-safe <see cref="IDisposable"/> class that invokes an
	/// <see cref="Action"/> in <see cref="IDisposable.Dispose"/>. Will not invoke the
	/// delegate twice. This generic class adds a <see cref="State"/> object that you
	/// can provide from a delegate when this object is constructed.
	/// </summary>
	/// <typeparam name="TState">Your state type.</typeparam>
	public abstract class DelegateDisposable<TState>
			: DelegateDisposable
	{
		/// <summary>
		/// Your arbitrary state. Set in the constructor; and set to default in dispose.
		/// </summary>
		public TState State { get; set; }
	}
}
