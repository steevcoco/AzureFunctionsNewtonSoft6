using System;
using System.Threading;
using Sc.Threading;


namespace Sc.Azure.Helpers
{
	/// <summary>
	/// Wraps a resource and a <see cref="SyncLock"/>.
	/// </summary>
	/// <typeparam name="TResource">The resource type.</typeparam>
	public class ResourceLock<TResource>
			: IDisposable
	{
		private bool isDisposed;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="resource">Not null.</param>
		/// <param name="disposeInFinalizer">If set true --- the default --- then the resource
		/// will be disposed if this object is finalized.</param>
		/// <param name="disposeWriteLockTimeout">This is a timeout that will be used in the
		/// Dispose method to obtain the writer lock. If the lock is not obtained, then the resource
		/// is still disposed if the <c>disposeWithoutLock</c> argument is true.</param>
		/// <param name="disposeWithoutLock">Applies if the write lock is not obtained within the
		/// timeout in the Dispose method: if this is true, the resource willbe disposed even if
		/// the lock is not acquired.</param>
		public ResourceLock(
				TResource resource,
				bool disposeInFinalizer = true,
				int disposeWriteLockTimeout = 30000,
				bool disposeWithoutLock = false)
		{
			if (resource == null)
				throw new ArgumentNullException(nameof(resource));
			Resource = resource;
			DisposeInFinalizer = disposeInFinalizer;
			DisposeWriteLockTimeout = disposeWriteLockTimeout;
			DisposeWithoutLock = disposeWithoutLock;
			SyncLock = new AsyncReaderWriterLock();
		}


		/// <summary>
		/// Is the locked resource. Notice that this is set null when this is disposed.
		/// </summary>
		public TResource Resource { get; private set; }

		/// <summary>
		/// The value set in the constructor.
		/// </summary>
		public bool DisposeInFinalizer { get; }

		/// <summary>
		/// The value set in the constructor.
		/// </summary>
		public int DisposeWriteLockTimeout { get; }

		/// <summary>
		/// The value set in the constructor.
		/// </summary>
		public bool DisposeWithoutLock { get; }

		/// <summary>
		/// The sync lock. Notice that this is NOT reentrant. Not null.
		/// </summary>
		public AsyncReaderWriterLock SyncLock { get; }

		/// <summary>
		/// Set true when disposed;
		/// </summary>
		public bool IsDisposed
		{
			get {
				Interlocked.MemoryBarrier();
				return isDisposed;
			}
		}


		protected virtual void Dispose(bool isDisposing)
		{
			using (SyncLock.WithWriteLock(out bool gotLock, DisposeWriteLockTimeout)) {
				if (!gotLock) {
					//TraceSources.For<ResourceLock<TResource>>()
					//		.Warning(
					//				$"Did not get write lock to dispose {this}."
					//				+ (DisposeWithoutLock
					//						? " Will be disposed anyway --- without the lock."
					//						: "Will NOT dispose."));
					if (!DisposeWithoutLock)
						return;
				}
				if (isDisposed)
					return;
				isDisposed = true;
				if (isDisposing
						|| DisposeInFinalizer)
					(Resource as IDisposable)?.Dispose();
				Resource = default;
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		~ResourceLock()
			=> Dispose(false);
	}
}
