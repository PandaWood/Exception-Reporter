﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

namespace ExceptionReporting.Tests
{
	public class DisposableTests
	{
		[SetUp]
		public void SetUp()
		{
			DisposeUnmanagedResourcesCalled = false;
			DisposeManagedResourcesCalled = false;
		}

		private static bool DisposeUnmanagedResourcesCalled { get; set; }
		private static bool DisposeManagedResourcesCalled { get; set; }

		private class DisposableStub : Disposable
		{
			protected override void DisposeManagedResources()
			{
				DisposeManagedResourcesCalled = true;
			}

			protected override void DisposeUnmanagedResources()
			{
				DisposeUnmanagedResourcesCalled = true;
			}
		}

		[Test]
		public void Test_Disposable()
		{
			var disposable = new DisposableStub();
			disposable.Dispose();
			Assert.IsTrue(disposable.IsDisposed);
			Assert.IsTrue(DisposeManagedResourcesCalled);
			Assert.IsTrue(DisposeUnmanagedResourcesCalled);
		}
	}
}