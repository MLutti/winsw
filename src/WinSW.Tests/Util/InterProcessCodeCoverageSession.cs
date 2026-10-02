using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Utilities.DbgEng;
using WinSW.Native;
using Xunit;

namespace WinSW.Tests.Util
{
    internal sealed class InterProcessCodeCoverageSession : IDebugEventCallbacks
    {
        private readonly Type trackerType;
        private readonly FieldInfo hitsField;

        private readonly IDebugControl control;
        private readonly DataTarget target;
        private readonly Thread thread;

        private List<Exception> exceptions;
        private bool exited;

        internal InterProcessCodeCoverageSession(string serviceName)
        {
            var trackerType = this.trackerType = typeof(Program).Assembly.GetTypes().Single(type => type.Namespace == "Coverlet.Core.Instrumentation.Tracker");
            var hitsField = this.hitsField = trackerType.GetField("HitsArray", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(hitsField);

            using var scm = ServiceManager.Open(ServiceApis.ServiceManagerAccess.Connect);
            using var sc = scm.OpenService(serviceName, ServiceApis.ServiceAccess.QueryStatus);

            int processId = sc.ProcessId;
            Assert.True(processId >= 0);

            var dbgeng = IDebugClient.Create();
            var client = (IDebugClient)dbgeng;
            this.control = (IDebugControl)dbgeng;

            int hr = client.AttachProcess(processId, DEBUG_ATTACH.DEFAULT);
            AssertEx.Succeeded(hr);

            hr = client.SetEventCallbacks(this);
            AssertEx.Succeeded(hr);

            target = DbgEngIDataReader.CreateDataTarget(dbgeng);

            var thread = this.thread = new Thread(() =>
            {
                try
                {
                    using (this.target)
                    {
                        do
                        {
                            int hr = this.control.WaitForEvent(TimeSpan.MaxValue);
                            AssertEx.Succeeded(hr);
                        }
                        while (!this.exited);
                    }
                }
                catch (Exception e)
                {
                    (this.exceptions ??= new List<Exception>()).Add(e);
                }
            });
            thread.Start();
        }

        /// <exception cref="AggregateException" />
        internal void Wait()
        {
            this.thread.Join();

            if (this.exceptions != null)
            {
                throw new AggregateException(this.exceptions);
            }
        }

        DEBUG_EVENT IDebugEventCallbacks.EventInterestMask => DEBUG_EVENT.EXIT_PROCESS;

        DEBUG_STATUS IDebugEventCallbacks.OnBreakpoint(IntPtr breakpoint) => throw new NotImplementedException();

        DEBUG_STATUS IDebugEventCallbacks.OnException(in EXCEPTION_RECORD64 exception, bool firstChance) => throw new NotImplementedException();

        DEBUG_STATUS IDebugEventCallbacks.OnCreateThread(ulong handle, ulong dataOffset, ulong startOffset) => throw new NotImplementedException();

        DEBUG_STATUS IDebugEventCallbacks.OnExitThread(int exitCode) => throw new NotImplementedException();

        DEBUG_STATUS IDebugEventCallbacks.OnCreateProcess(ulong imageFileHandle, ulong handle, ulong baseOffset, uint moduleSize, string moduleName, string imageName, uint checkSum, uint timeDateStamp, ulong initialThreadHandle, ulong threadDataOffset, ulong startOffset) => throw new NotImplementedException();

        DEBUG_STATUS IDebugEventCallbacks.OnExitProcess(int exitCode)
        {
            this.exited = true;

            try
            {
                using var runtime = this.target.ClrVersions.Single().CreateRuntime();

                ClrModule module = runtime.EnumerateModules().First(module => module.Name == typeof(Program).Assembly.Location);

                var type = module.GetTypeByName(this.trackerType.FullName);
                var field = type.GetStaticFieldByName(this.hitsField.Name);
                var array = field.ReadObject(runtime.AppDomains.Single()).AsArray();

                int[] hits = (int[])this.hitsField.GetValue(null);

                int[] values = array.ReadValues<int>(0, hits.Length);
                for (int i = 0; i < hits.Length; i++)
                {
                    hits[i] += values[i];
                }
            }
            catch (Exception e)
            {
                (this.exceptions ??= new List<Exception>()).Add(e);
            }

            return DEBUG_STATUS.BREAK;
        }

        DEBUG_STATUS IDebugEventCallbacks.OnLoadModule(ulong imageFileHandle, ulong baseOffset, uint moduleSize, string moduleName, string imageName, uint checkSum, uint timeDateStamp) => throw new NotImplementedException();

        DEBUG_STATUS IDebugEventCallbacks.OnUnloadModule(string imageBaseName, ulong baseOffset) => throw new NotImplementedException();

        DEBUG_STATUS IDebugEventCallbacks.OnSystemError(uint error, uint level) => throw new NotImplementedException();

        DEBUG_STATUS IDebugEventCallbacks.OnSessionStatus(DEBUG_SESSION status) => throw new NotImplementedException();

        DEBUG_STATUS IDebugEventCallbacks.OnDebuggeeChangeState(DEBUG_CDS flags, ulong argument) => throw new NotImplementedException();

        DEBUG_STATUS IDebugEventCallbacks.OnEngineChangeState(DEBUG_CES flags, ulong argument) => throw new NotImplementedException();

        DEBUG_STATUS IDebugEventCallbacks.OnSymbolChangeState(DEBUG_CSS flags, ulong argument) => throw new NotImplementedException();
    }
}
