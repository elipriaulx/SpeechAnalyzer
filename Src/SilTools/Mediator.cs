// --------------------------------------------------------------------------------------------
#region // Copyright (c) 2003, SIL International. All Rights Reserved. 
// <copyright from='2003' to='2003' company='SIL International'>
//		Copyright (c) 2003, SIL International. All Rights Reserved.   
//    
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright> 
#endregion
// 
// File: Mediator.cs
// Authorship History: John Hatton  
// Last reviewed: 
// 
// <remarks>
// </remarks>
// --------------------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SilTools
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// 
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public interface IxCoreColleague
	{
		IxCoreColleague[] GetMessageTargets();
	}
	
	/// <summary>
	/// Add this attribute to a class to say the mediator should dispose of it
	/// if it is still a coleague of the mediator when that is disposed.
	/// "Listeners" should normally have this.
	/// </summary>
	public class MediatorDisposeAttribute : Attribute
	{
	}

	/// --------------------------------------------------------------------------------------------
	/// <summary>
	/// Summary description for Mediator.
	/// </summary>
	/// --------------------------------------------------------------------------------------------
	public sealed class Mediator : IDisposable
	{
		#region PendingMessageItem

		/// <summary>
		/// This class is used by the message queue system.
		/// </summary>
		private class PendingMessageItem
		{
			public string m_message;
			public object m_parameter;

			public PendingMessageItem(string message, object parameter)
			{
				m_message = message;
				m_parameter = parameter;
			}
		}

		#endregion #region PendingMessageItem

		#region QueueItem class
		/// <summary>
		/// This is a simple class that contains the information that is used in the InvokeRecursively method.
		/// </summary>
		private class QueueItem
		{
			public string m_methodName;
			public Type[] m_parameterTypes;
			public object[] m_parameterList;
			public bool m_stopWhenHandled;
			public bool m_justCheckingForReceivers;

			public QueueItem(string methodName, Type[] parameterTypes, object[] parameterList, bool stopWhenHandled, bool justCheckingForReceivers)
			{
				m_methodName = methodName;
				m_parameterTypes = parameterTypes;
				m_parameterList = parameterList;
				m_stopWhenHandled = stopWhenHandled;
				m_justCheckingForReceivers = justCheckingForReceivers;
			}

			public override bool Equals(object obj)
			{
				if (obj == null)
					return false;
				if (!(obj is QueueItem))
					return false;

				QueueItem asItem = obj as QueueItem;
				if (m_methodName != asItem.m_methodName)
					return false; // Not the same nethod.
				if (m_parameterTypes.Length != asItem.m_parameterTypes.Length)
					return false; // Not the same number of parameter types.
				if (m_parameterList.Length != asItem.m_parameterList.Length)
					return false; // Not the same number of parameter values.
				if (m_parameterTypes.Length != asItem.m_parameterList.Length)
					return false; // Not the same number of parameter types to values.
				for (int i = 0; i < m_parameterTypes.Length; ++i)
				{
					Type parmType = m_parameterTypes[i];
					object parmValue = m_parameterList[i];
					if (asItem.m_parameterTypes[i] != parmType || asItem.m_parameterList[i] != parmValue)
						return false;
				}
				return true;
			}

			public override int GetHashCode()
			{
				int hashCode = m_methodName.GetHashCode();

				foreach (Type typ in m_parameterTypes)
					hashCode += typ.GetHashCode();
				foreach (object obj in m_parameterList)
					hashCode += obj.GetHashCode();

				return hashCode;
			}

			public override string ToString()
			{
				StringBuilder sb = new StringBuilder("QueueItem: ");
				sb.AppendFormat("{0}:", m_methodName);
				for (int i = 0; i < m_parameterTypes.Length; ++i)
				{
					sb.AppendFormat(" {0}:{1}", m_parameterTypes[i].ToString(), m_parameterList[i].ToString());
				}
				return sb.ToString();
			}
		}
		#endregion

		#region Data members
		
		// testing to have list of IxCoreColleagues that are disposed now
		private HashSet<string> m_disposedColleagues = new HashSet<string>();
		
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds the disposed colleague.
		/// </summary>
		/// <param name="hashKey">The hash key.</param>
		/// ------------------------------------------------------------------------------------
		public void AddDisposedColleague(string hashKey)
		{
			CheckDisposed();
			
			if (!m_disposedColleagues.Contains(hashKey))
			{
				m_disposedColleagues.Add(hashKey);
				Debug.WriteLine("@@Added "+hashKey);
			}
		}

		public bool IsDisposedColleague(string hashKey) { return m_disposedColleagues.Contains(hashKey); }
		private bool m_processMessages = true;
		private HashSet<IxCoreColleague> m_colleagues = new HashSet<IxCoreColleague>();
		private IxCoreColleague m_temporaryColleague;
		private Dictionary<string, string> m_pathVariables = new Dictionary<string, string>();
		/// <summary>
		/// Control how much output we send to the application's listeners (e.g. visual studio output window)
		/// </summary>
		private TraceSwitch showPendingMsgsSwitch = new TraceSwitch("ShowPendingMsgs", "All Items on the pending msg queue", "Off");
		private TraceSwitch invokeSwitch = new TraceSwitch("XCore.Mediator_InvokeTrace", "Invoke tracking", "Off");
		#region Data members for Queueing
		/// <summary>
		/// variables for a pending message, to be handled in the idle loop.
		/// </summary>
		private Queue<PendingMessageItem> m_pendingMessages = new Queue<PendingMessageItem>();
		/// <summary>This is the message value that is used to communicate the need to process the defered mediator queue</summary>
		public const int WM_BROADCAST_ITEM_INQUEUE = 0x8000+0x77;	// wm_app + 0x77
		/// <summary>Included here so as to not add another common utils dependancy</summary>
#if !__MonoCS__
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool PostMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam);
#else
		public static bool PostMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam)
		{
			Console.WriteLine("Warning--using unimplemented method PostMessage"); // FIXME Linux
			return(false);
		}
#endif
		private Queue<QueueItem> m_jobs = new Queue<QueueItem>();	// queue to contain the defered broadcasts
		private Queue<QueueItem> m_pendingjobs = new Queue<QueueItem>(); // queue to contain the broadcasts until we have a main window
		private int m_queueLastProcessed;	// size of queue the last time it was processed
		private bool m_RemovedItemLast;
		// flag to let us know when we're already processing an item
		private bool m_processItemBusy;

		#endregion
		private long m_SavedCalls;	// number of calls to Type.GetMethod that are saved (just informational).
		private long m_MethodsCount;	// max depth on the methods of all colleagues
#if DEBUG
		private long m_MethodChecks;	// total number of calls to the IsMethodNOTonColleague method
#endif

		/// <summary>keeps a list of classes (colleagues) and the methods that it doesn't contain</summary>
		private Dictionary<string, HashSet<string>> m_MethodsNOTonColleagues;	// key=colleague.ToString(), value=Set of methods of methods
		/// <summary>Set of method names that are implemented by any colleague</summary>
		private HashSet<string> m_MethodsOnAnyColleague;
		#endregion

		#region Construction and Initialization
		/// -----------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="Mediator"/> class.
		/// </summary>
		/// -----------------------------------------------------------------------------------
		public Mediator()
		{
			m_MethodsNOTonColleagues = new Dictionary<string, HashSet<string>>();
			m_MethodsOnAnyColleague = new HashSet<string>();

			// NOTE: to set the trace level, create a config file like the following and set 
			// it there. Values are: Off=0, Error=1, Warning=2, Info=3, Verbose=4.
			// <configuration>
			//    <system.diagnostics>
			//       <switches>
			//          <add name="Mediator" value="3" />
			//       </switches>
			//    </system.diagnostics>
			// </configuration>

			// OR you can temporarily uncomment one of the following:

			//invokeSwitch.Level = TraceLevel.Info;	//some output. Will tell you what methods are being invoke, on whom.

			//NB: To make this much use, you'll need to disable the IDLE processing in XWindow.
			//otherwise, XCore will spend all of its time tracing things here during idle time 
			//	and you won't be able to find the bug You are seeking.

			/* -----------------------------------------------------------------
			 * TraceLevel.Verbose will show something like this (BUT SO MUCH IT'S HARDLY USEABLE):
			  
			  Looking for listeners for Msg: DisplayWritingSystemList
				[XCore.MockupDialogLauncher
					{XCore.MockupDialogLauncher
						Checking : XCore.MockupDialogLauncher
					}
				]
				[SIL.FieldWorks.WordWorks.Parser.ParserListener
					{SIL.FieldWorks.WordWorks.Parser.ParserListener
						Checking : SIL.FieldWorks.WordWorks.Parser.ParserListener
					}
				]

				[SIL.FieldWorks.XWorks.WritingSystemListHandler
					{SIL.FieldWorks.XWorks.WritingSystemListHandler
						Checking : SIL.FieldWorks.XWorks.WritingSystemListHandler
						Invoking method: SIL.FieldWorks.XWorks.WritingSystemListHandler
						Invoking Method: OnDisplayWritingSystemList on SIL.FieldWorks.XWorks.WritingSystemListHandler
					}
				]
			*/

		}

		#endregion

		#region IDisposable & Co. implementation
		// Region last reviewed: Oct. 13, 2005 (RandyR).

		/// <summary>
		/// True, if the object has been disposed.
		/// </summary>
		private bool m_isDisposed;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Throw if the IsDisposed property is true
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void CheckDisposed()
		{
			if (m_isDisposed)
				throw new ObjectDisposedException("Mediator", "This object is being used after it has been disposed: this is an Error.");
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		/// <remarks>
		/// In case some clients forget to dispose it directly.
		/// </remarks>
		/// ------------------------------------------------------------------------------------
		~Mediator()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <remarks>Must not be virtual.</remarks>
		/// ------------------------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue 
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Executes in two distinct scenarios.
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
		/// <c>false</c> to release only unmanaged resources.</param>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the bug.
		/// If subclasses override this method, they should call the base implementation.
		/// </remarks>
		/// ------------------------------------------------------------------------------------
		private void Dispose(bool disposing)
		{
			//Debug.WriteLineIf(!disposing, "****************** " + GetType().Name + " 'disposing' is false. ******************");
			// Can be called more than once, but not run more than once.
			if (m_isDisposed)
				return;

			if (disposing)
			{
				// Dispose managed resources here.

				// Use a copy of the m_colleagues Set,
				// since the Dispose methods on the colleague should remove itself from m_colleagues,
				// which will cause an exception to be throw (list changed while spinning through it.
				HashSet<IxCoreColleague> copyOfColleagues = new HashSet<IxCoreColleague>(m_colleagues);
				m_colleagues.Clear(); // Just get rid of them now.
				foreach (IxCoreColleague icc in copyOfColleagues)
				{
					if (icc is IDisposable)
					{
						// Is the class marked with [XCore.MediatorDispose],
						// or is it in the temporary colleague holder?
						object[] attrs = icc.GetType().GetCustomAttributes(typeof(MediatorDisposeAttribute), true);
						if ((attrs != null && attrs.Length > 0) || m_temporaryColleague == icc)
							(icc as IDisposable).Dispose();
					}
				}
				copyOfColleagues.Clear();

				if (m_pathVariables != null)
					m_pathVariables.Clear();
				if (m_disposedColleagues != null)
					m_disposedColleagues.Clear();
				if (m_pendingMessages != null)
				{
					m_pendingMessages.Clear();
					// Since we have gotten rid of the pending messages, also remove the event handler.
					// Otherwise you can get a null exception when trying to process the idle event: LT-5830
					Application.Idle -= PostMessageApplication_Idle;
				}
				if (m_jobs != null)
					m_jobs.Clear();
				if (m_pendingjobs != null)
					m_pendingjobs.Clear();
				if (m_MethodsNOTonColleagues != null)
					m_MethodsNOTonColleagues.Clear();
				if (m_MethodsOnAnyColleague != null)
					m_MethodsOnAnyColleague.Clear();
			}

			// Dispose unmanaged resources here, whether disposing is true or false.
			m_mainWndPtr = IntPtr.Zero;
			m_MethodsOnAnyColleague = null;
			m_MethodsNOTonColleagues = null;
			m_pendingjobs = null;
			m_jobs = null;
			m_pendingMessages = null;
			m_disposedColleagues = null;
			m_temporaryColleague = null;
			m_colleagues = null;
			m_pathVariables = null;
/* It is illegal to try to access managed stuff in this part of the Dispose method.
#if DEBUG
			//DebugMsg("-- Number of calls to the InvokeRecursively method = " + m_invokeCount.ToString());
			DebugMsg("-- Number of saved calls to Type.GetMethod = " + m_SavedCalls.ToString());
			DebugMsg("-- Mediator MsgHash info: count=" + m_MethodsNOTonColleagues.Count + " mx depth=" + m_MethodsCount);
			DebugMsg("-- Mediator  - Calls to check for method on colleague: " + m_MethodChecks);
#endif
*/
			m_isDisposed = true;
		}

		#endregion IDisposable & Co. implementation

		#region Defered processing Queue items & implementation
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Retrieve a copy of the current queue of pending messages
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns>
		/// 	<c>true</c> if [is message in pending queue] [the specified message]; otherwise, <c>false</c>.
		/// </returns>
		/// ------------------------------------------------------------------------------------
		public bool IsMessageInPendingQueue(string message)
		{
			CheckDisposed();
			foreach (PendingMessageItem pmi in m_pendingMessages.ToArray())
			{
				if (pmi.m_message == message)
					return true;
			}
			return false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether [process messages].
		/// </summary>
		/// <value><c>true</c> if [process messages]; otherwise, <c>false</c>.</value>
		/// ------------------------------------------------------------------------------------
		public bool ProcessMessages
		{
			get
			{
				CheckDisposed();
				return m_processMessages;
			}
			set
			{
				CheckDisposed();
				m_processMessages = value;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Set the handle for the main window
		/// </summary>
		/// <value>The main window.</value>
		/// ------------------------------------------------------------------------------------
		public Form MainWindow
		{
			set
			{
				CheckDisposed();

				if (value == null)
					m_mainWndPtr = IntPtr.Zero;
				else if (value.IsHandleCreated)
					m_mainWndPtr = value.Handle;
				else
					throw new ArgumentException("Form has to have valid handle to use this property.");
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Set this to true immediately if the mediator needs to post messages to one specific window.  If this is set
		/// to true, then you must set the MainWindow property to the handle of the main window.  Then, after the application
		/// has sufficiently initialized, call the BroadcastPendingItems method.  Note that while the OnHandleCreated method
		/// is a good place to set the MainWindow property, it _will not_ work for calling BroadcastPendingItems().
		/// BroadcastPendingItems() will need to be called some time after the OnHandleCreated method is finished.
		/// No broadcasts will be sent until both MainWindow has been set and BroadcastPendingItems() has been called.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [specific to one main window]; otherwise, <c>false</c>.
		/// </value>
		/// ------------------------------------------------------------------------------------
		public bool SpecificToOneMainWindow
		{
			set
			{
				CheckDisposed();

				// If we are specific to one main window, then it is not safe to broadcast until BroadcastPendingItems() is called
				m_specificToOneMainWindow = value;
				m_safeToBroadcast = !value;
			}
		}

		// flag set if we are going to have a specific m_mainWindowHandler but don't yet.
		bool m_specificToOneMainWindow = false;
		private bool m_safeToBroadcast = true;
		private IntPtr m_mainWndPtr = IntPtr.Zero;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This posts a WM_BROADCAST... msg to the main app window
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void AddWindowMessage()
		{
			if (!ProcessMessages)
				return;

			// If the application's main windows all use the same mediator (e.g., apps like TE based
			// on FwMainWindow), m_mainWndPtr can be left zero; whatever is the current main window
			// can be allowed to send the ProcessMessages call back to its mediator (which will be this).
			// If different windows use different mediators, then each mediator needs to know which main window
			// it can post messages to. This is currently set up when xWindow creates a mediator;
			// if any other base class is used for windows which have a mediator each, another means will
			// be needed.
			IntPtr mainWndPtr;
			if (m_specificToOneMainWindow)
				mainWndPtr = m_mainWndPtr;
			else
				mainWndPtr = Process.GetCurrentProcess().MainWindowHandle;
			PostMessage(mainWndPtr, WM_BROADCAST_ITEM_INQUEUE, 0, 0);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Add an item to the queue and let the app know an item is present to be processed.
		/// </summary>
		/// <param name="item">The item.</param>
		/// ------------------------------------------------------------------------------------
		private void AddQueueItem(QueueItem item)
		{
			if (m_specificToOneMainWindow && (m_mainWndPtr == IntPtr.Zero || !m_safeToBroadcast))
			{
				//if (!m_pendingjobs.Contains(item))
				//{
					m_pendingjobs.Enqueue(item);
				//}
//#if DEBUG
//                else
//                {
//                    Debug.WriteLine("Found duplicate pending jobs: " + item.ToString());
//                }
//#endif
			}
			else
			{
				// Review TE team(JohnT/CurtisH): do you need to use pending jobs if Process.GetCurrentProcess().MainWindowHandle is zero?
				//if (!m_jobs.Contains(item))
				//{
					m_jobs.Enqueue(item);	// add the item to the queue
					m_RemovedItemLast = false;
					AddWindowMessage();		// now post the msg to allow future processing of the queue
				//}
//#if DEBUG
//                else
//                {
//                    Debug.WriteLine("Found duplicate jobs: " + item.ToString());
//                }
//#endif

			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Broadcasts the pending items.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void BroadcastPendingItems()
		{
			CheckDisposed();

			if (!ProcessMessages)
				return;

			// Make a copy of the queue to prevent an infinitely long queue where we just add the items back into the queue
			// inside the AddQueueItem as we dequeue them here.
			Queue<QueueItem> pendingjobs = new Queue<QueueItem>(m_pendingjobs.ToArray());

			// We can actually broadcast now
			m_safeToBroadcast = true;

			while (pendingjobs.Count > 0)
			{
				QueueItem pending = m_pendingjobs.Dequeue();
				QueueItem pendingCopy = pendingjobs.Dequeue();
				Debug.Assert(pending == pendingCopy);
				AddQueueItem(pendingCopy);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the job items.
		/// </summary>
		/// <value>The job items.</value>
		/// ------------------------------------------------------------------------------------
		private int JobItems
		{ 
			get { return m_jobs.Count; }
		}	// queue count

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Debugging method to show the items on the queue that are new.
		/// </summary>
		/// <param name="startPos">The start pos.</param>
		/// ------------------------------------------------------------------------------------
		private void ShowJobItems(int startPos)
		{
			if (showPendingMsgsSwitch.Level == TraceLevel.Verbose)	// only do this if verbose setting
			{
				int pos = 0;
				foreach (QueueItem item in m_jobs)
				{
					if (pos >= startPos || m_RemovedItemLast)
					{
						Trace.WriteLine("Job Queue: " + pos + " - [" +
							item.m_methodName + "]<" +
							(item.m_parameterList[0] == null ? "NULL" : item.m_parameterList[0].ToString()) + ">",
							showPendingMsgsSwitch.DisplayName);
					}
					pos++;
				}
			}
		}

//		[Conditional("DEBUG")]
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Debugs the MSG.
		/// </summary>
		/// <param name="msg">The MSG.</param>
		/// ------------------------------------------------------------------------------------
		private void DebugMsg(string msg)
		{
			// create the initial info:
			// datetime threadid threadpriority: msg
			StringBuilder msgOut = new StringBuilder();
			msgOut.Append(DateTime.Now.Ticks);
			msgOut.Append("-");
			msgOut.Append(Thread.CurrentThread.GetHashCode());
			msgOut.Append("-");
			msgOut.Append(Thread.CurrentThread.Priority);
			msgOut.Append(": ");
			msgOut.Append(msg);
			Debug.WriteLine(msgOut.ToString());
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Builds the debug MSG.
		/// </summary>
		/// <param name="msg">The MSG.</param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		private string BuildDebugMsg(string msg)
		{
			// create the initial info:
			// datetime threadid threadpriority: msg
			StringBuilder msgOut = new StringBuilder();
			msgOut.Append(DateTime.Now.Ticks);
			msgOut.Append("-");
			msgOut.Append(Thread.CurrentThread.GetHashCode());
			msgOut.Append("-");
			msgOut.Append(Thread.CurrentThread.Priority);
			msgOut.Append(": ");
			msgOut.Append(msg);
			return msgOut.ToString();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Process one of the items in the deferred queue and don't be re-entrant.
		/// </summary>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		public bool ProcessItem()
		{
			CheckDisposed();

			if (!ProcessMessages)
				return true;

			bool success = true;	// by default we set our status to having processed an item
			try
			{
				if (m_processItemBusy)	// we're already working, so ignore this call (it will come back later).
				{
					Trace.WriteLineIf(showPendingMsgsSwitch.TraceInfo, 
						BuildDebugMsg("Mediator::ProcessItem is busy, come back later."),
						showPendingMsgsSwitch.DisplayName);
					success = false;	// don't reset the busy flag on the way out
					return false;
				}
				m_processItemBusy = true;	// we're now processing an item so set the flag
				if (JobItems == 0)		// only process if the queue isn't empty
				{
					Trace.WriteLineIf(showPendingMsgsSwitch.TraceInfo,
						BuildDebugMsg("Job Queue: Empty"),
						showPendingMsgsSwitch.DisplayName);
					return false;
				}
				m_RemovedItemLast = true;

				// show the queue if it has grown sense last processed (1 or more new itesm added)
				if (m_queueLastProcessed < JobItems)
					ShowJobItems(m_queueLastProcessed);
			
				QueueItem item = m_jobs.Dequeue();	// extract the item
				m_queueLastProcessed = JobItems;	// save the count for future use
				if (item == null)
					return false;

				Trace.WriteLineIf(showPendingMsgsSwitch.TraceInfo,
					BuildDebugMsg("Job Queue: Processing - [" +	item.m_methodName + "]<" +
					(item.m_parameterList[0] == null ? "NULL" : item.m_parameterList[0].ToString()) + ">"),
					showPendingMsgsSwitch.DisplayName);

				// now call invoke method
				bool rval = InvokeOnColleagues(item.m_methodName, item.m_parameterTypes, item.m_parameterList, item.m_stopWhenHandled, item.m_justCheckingForReceivers);
				AddWindowMessage();	// make sure windows doesn't combine 'n' of our messages into one
				return rval;		// result of invoke method
			}
			finally
			{
				// always be sure to turn off the processing flag if we're done (success==true)
				if (success)
					m_processItemBusy = false;
			}
		}
		#endregion

		#region MethodNOTonColleague helper methods
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Keep a running list of methods that don't exist on objects.  This is static during the life of the object.
		/// Also assumes that the cost of storing the data structures are cheaper than the repeated calls to try to
		/// invoke methods that don't exist.
		/// </summary>
		/// <param name="method">name of method(EX:OnPropertyChanged)</param>
		/// <param name="colleague">name of class(EX:XCore.MockupDialogLauncher)</param>
		/// ------------------------------------------------------------------------------------
		private void AddMethodNOTonColleague(string method, string colleague)
		{
			HashSet<string> methods = null;
			if (!m_MethodsNOTonColleagues.TryGetValue(colleague, out methods))
			{
				methods = new HashSet<string>();
				m_MethodsNOTonColleagues[colleague] = methods;
			}

			if (methods.Count > m_MethodsCount)
				m_MethodsCount = methods.Count;	// max depth on the methods of all colleagues
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This routine will check the hash of classes (colleagues) and see if the method is listed as not
		/// being defined for the given class.
		/// </summary>
		/// <param name="method">The method.</param>
		/// <param name="colleague">The colleague.</param>
		/// <returns>
		/// 	<c>true</c> if [is method NO ton colleague] [the specified method]; otherwise, <c>false</c>.
		/// </returns>
		/// ------------------------------------------------------------------------------------
		private bool IsMethodNOTonColleague(string method, string colleague)
		{
#if DEBUG
			m_MethodChecks++;	// just for logging
#endif
			return m_MethodsNOTonColleagues.ContainsKey(colleague)
				&& m_MethodsNOTonColleagues[colleague].Contains(method);
		}
		#endregion

		#region command dispatching stuff
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// call this method on any object (which implements it) that we can find
		/// Note that unlike SendMessage(),
		/// this does not care if anyone claims to have "handled" the message.
		/// It will keep sending messages to everyone.
		/// </summary>
		/// <param name="methodName">Name of the method.</param>
		/// <param name="stringParam">The string param.</param>
		/// ------------------------------------------------------------------------------------
		public void BroadcastString(string methodName, string stringParam)
		{
			CheckDisposed();

			if (!ProcessMessages)
				return;

			if (m_colleagues.Count == 0)
				return;		// no one to broadcast to...
#if false
			TraceVerboseLine("Broadcasting  "+methodName);
			InvokeOnColleagues(methodName, new Type[] {typeof(string)}, new Object[] { stringParam }, false, false);		
#else
			AddQueueItem(new QueueItem(methodName, new Type[] {typeof(string)}, new Object[] { stringParam }, false, false));
#endif
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This version is used to invoke commands
		/// </summary>
		/// <param name="messageName"></param>
		/// <param name="parameter"></param>
		/// <returns><c>true</c> if the message was handled, otherwise <c>false</c></returns>
		/// ------------------------------------------------------------------------------------
		public bool SendMessage(string messageName, object parameter)
		{
			CheckDisposed();

			if (!ProcessMessages)
				return true;

			bool result;
			try
			{
				// Have seen a stack situation that the Mediator has been disposed of after returning from this call...
				result = SendMessage(messageName, parameter, true);
			}
			catch (DisposedInAnotherFrameException)
			{
				Debug.WriteLine("EXCEPTION: Cought case where the Mediator was disposed of while processing...");
				result = true;	// don't process any more
			}
			return result;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This version is used to invoke commands
		/// </summary>
		/// <param name="messageName"></param>
		/// <param name="parameter"></param>
		/// <param name="fLogIt">True to log the call (if its not an update or idle), false
		/// otherwise</param>
		/// <returns><c>true</c> if the message was handled, otherwise <c>false</c></returns>
		/// ------------------------------------------------------------------------------------
		public bool SendMessage(string messageName, object parameter, bool fLogIt)
		{
			CheckDisposed();

			if (!ProcessMessages)
				return true;
#if DEBUG
			if(messageName.Substring(0,2) == "On")
				Debug.Fail("The convention is to send messages without the 'On' prefix. " +
					"That is added by the message sending code.");
#endif
			if (messageName != "Idle")
			{
				Trace.WriteLineIf(showPendingMsgsSwitch.TraceVerbose,
					BuildDebugMsg("Looking for listeners for Msg: " + messageName),
					showPendingMsgsSwitch.DisplayName);
			}
			string methodName = "On" + messageName;
			// Logging 
			if (!messageName.StartsWith("Update") && messageName != "Idle" && fLogIt)
			{
				// We want to log the method if any colleague handles it.
				// So we check the list of methods known-to-us first. If we don't find it,
				// we check if any colleague implements it by pretending to invoke it.
				// If any colleague implements it, we'll log it; if nobody implements it,
				// it's no use to go any further.
				if (!m_MethodsOnAnyColleague.Contains(methodName))
				{
					// Didn't find it so far, so it must be a method not previously called.
					// Check if anybody implements it.
					if (!InvokeOnColleagues(methodName, new[] {typeof(object)},
						new[] { new Object() }, true, true))		//we are just checking, don't invoke anything
					{
						return false;
					}
				}
				Log("Invoked " + messageName);
			}

			return InvokeOnColleagues(methodName, new[] { typeof(object) }, 
				new[] { parameter }, true, false);		
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This method is a cross of the broadcast and sendmessage types.  It will send
		/// the message to everyone right now with out stoping when it's handled.  This is
		/// provided for places where the previous broadcast functionality is needed, or
		/// when it's not clear and is desired as a first step.
		/// </summary>
		/// <param name="messageName">Name of the message.</param>
		/// <param name="parameter">The parameter.</param>
		/// <returns>
		/// true if one or more colleagues handled the message, else false
		/// </returns>
		/// ------------------------------------------------------------------------------------
		public bool SendMessageToAllNow(string messageName, object parameter)
		{
			CheckDisposed();

			if (!ProcessMessages)
				return true;

			return InvokeOnColleagues("On" + messageName, new[] {typeof(object)}, 
				new[] { parameter }, false, false);		
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This method is a replacement message for the SendMessage when the return value isn't
		/// actually used.  It allows those messages to be defered for a different message (later).
		/// </summary>
		/// <param name="messageName">Name of the message.</param>
		/// <param name="parameter">The parameter.</param>
		/// ------------------------------------------------------------------------------------
		public void SendMessageDefered(string messageName, object parameter)
		{
			CheckDisposed();

			if (!ProcessMessages)
				return;

#if DEBUG
			if(messageName.Substring(0,2) == "On")
				Debug.Fail("The convention is to send messages without the 'On' prefix. " +
					"That is added by the message sending code.");
#endif
			AddQueueItem(new QueueItem("On" + messageName, new[] {typeof(object)},
				new[] { parameter }, true, false));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Post a command to be sent during an upcoming idle moment.
		/// </summary>
		/// <param name="messageName"></param>
		/// <param name="parameter"></param>
		/// ------------------------------------------------------------------------------------
		public void PostMessage(string messageName, object parameter)
		{
			CheckDisposed();

			if (!ProcessMessages)
				return;

			// If the queue is empty then subscribe to the idle event to process messages
			if (m_pendingMessages.Count == 0)
				Application.Idle += PostMessageApplication_Idle;

			// Add this message to the queue
			m_pendingMessages.Enqueue(new PendingMessageItem(messageName, parameter));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Dispatch messages from the post queue
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void PostMessageApplication_Idle(object sender, EventArgs e)
		{
			try
			{
				// Dispatch all of the messages on the queue
				while (m_pendingMessages.Count > 0)
				{
					if (!ProcessMessages)
						return;

					PendingMessageItem pmi = m_pendingMessages.Dequeue();
					SendMessage(pmi.m_message, pmi.m_parameter); // could be defered?
				}
			}
			finally
			{
				// Since the queue is now empty, turn off the idle handler
				Application.Idle -= PostMessageApplication_Idle;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This version is used to broadcast changes that might affect several objects.
		/// Each of them gets to see it, even if one (or more) claim to have handled it.
		/// </summary>
		/// <param name="messageName"></param>
		/// <param name="parameter"></param>
		/// <returns><c>true</c> if the message was handled, otherwise <c>false</c></returns>
		/// ------------------------------------------------------------------------------------
		public bool BroadcastMessage(string messageName, object parameter)
		{
			CheckDisposed();

#if DEBUG
			if(messageName.Substring(0,2) == "On")
				Debug.Fail("The convention is to send messages without the 'On' prefix. " +
					"That is added by the message sending code.");
#endif
			if (messageName != "Idle")
			{
				Trace.WriteLineIf(showPendingMsgsSwitch.TraceVerbose,
					BuildDebugMsg("Looking for listeners for Msg: " + messageName),
					showPendingMsgsSwitch.DisplayName);
			}
#if false
			return InvokeOnColleagues("On" + messageName, new Type[] {typeof(object)}, 
				new Object[] { parameter }, false, false);		
#else
			AddQueueItem(new QueueItem("On"+messageName, new Type[] {typeof(object)}, 
				new[] { parameter }, false, false));
			return false;
#endif
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This is a deferred message, but it's not sent to everyone, it is sent until
		/// it is handled.
		/// </summary>
		/// <param name="messageName">Name of the message.</param>
		/// <param name="parameter">The parameter.</param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		public bool BroadcastMessageUntilHandled(string messageName, object parameter)
		{
			CheckDisposed();

#if DEBUG
			if(messageName.Substring(0,2) == "On")
				Debug.Fail("The convention is to send messages without the 'On' prefix. " +
					"That is added by the message sending code.");
#endif

			AddQueueItem(new QueueItem("On"+messageName, new Type[] {typeof(object)}, 
				new[] { parameter }, true, false));
			return false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Check to see if there would be someone who would receive this command if it was given right now.
		/// </summary>
		/// <param name="messageName">Name of the message.</param>
		/// <returns>
		/// 	<c>true</c> if the specified message name has receiver; otherwise, <c>false</c>.
		/// </returns>
		/// ------------------------------------------------------------------------------------
		public bool HasReceiver(string messageName)
		{
			CheckDisposed();

			if (messageName == null)
				return false; // As might be for a menu separator bar.
#if DEBUG
			if(messageName.Substring(0,2) == "On")
				Debug.Fail("The convention is to send messages without the 'On' prefix.  That is added by the message sending code.");
#endif
			if (messageName != "Idle")
			{
				Trace.WriteLineIf(showPendingMsgsSwitch.TraceVerbose,
					BuildDebugMsg("Checking for listeners for Msg: " + messageName),
					showPendingMsgsSwitch.DisplayName);
			}
							

			return InvokeOnColleagues("On"+messageName, new Type[] {typeof(object)},
				new[] { new Object() }, //we just make a dummy object here, since we won't really be invoking anything
				true,	//meaningless in this context
				true);		//we are just checking, don't invoke anything
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sends the cancellable message.
		/// </summary>
		/// <param name="messageName">Name of the message.</param>
		/// <param name="parameter">The parameter.</param>
		/// <returns>
		/// true if the message was canceled, otherwise false.
		/// </returns>
		/// ------------------------------------------------------------------------------------
		public bool SendCancellableMessage(string messageName, object parameter)
		{
			CheckDisposed();
#if DEBUG
			if(messageName.Substring(0,2) == "On")
				Debug.Fail("The convention is to send messages without the 'On' prefix.  That is added by the message sending code.");
#endif

			CancelEventArgs cancelArguments = new  CancelEventArgs(false);
			InvokeOnColleagues("On"+messageName, 
				null, // types see note above
				new[] { parameter, cancelArguments },
				true, false);		
			return cancelArguments.Cancel;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="parameterTypes"></param>
		/// <param name="parameterList"></param>
		/// <param name="stopWhenHandled"></param>
		/// <param name="justCheckingForReceivers"></param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		private bool InvokeOnColleagues(string methodName, Type[] parameterTypes, 
			object[] parameterList, bool stopWhenHandled, bool justCheckingForReceivers)
		{
			CheckDisposed();
			
			if (!ProcessMessages)
				return true;

			bool handled = false;
			if (invokeSwitch.TraceVerbose)
				Trace.Indent();
			Trace.WriteLineIf(invokeSwitch.TraceVerbose,
				BuildDebugMsg("InvokeOnColleagues: methodName=<" + methodName + ">"),
				invokeSwitch.DisplayName);

			try
			{
				// Don't need this assert, since it can only be null, if the Mediator has been disposed,
				// which will be caught higher up in this method.
				//Debug.Assert(m_colleagues != null);
				//we need to copy our list of colleagues because the list of colleagues may change while we are
				//iterating over this list, which is not allowed if we simply use an enumerater.
				IxCoreColleague[] targets = m_colleagues.ToArray();
				//to catch infinite loops
				HashSet<int> previous = new HashSet<int>(); // The int is the HashCode of some IxCoreColleague target.

				// ms-help://MS.MSDNQTR.v80.en/MS.MSDN.v80/MS.NETDEV.v10.en/dnpag/html/scalenetchapt13.htm
				// .."foreach introduces both managed heap and virtual function overhead..
				// This can be a significant factor in performance-sensitive regions of your application."
				// This section of code is indeed a very performance-sensitive region - so lets pull out
				// the foreach
				// Followup note by RandyR: by the time we get to .Net 2.0, such time savings have become myths,
				// at least for object arryas such as targets.
				// The jury is still out on any performance penalty of foreach when using generics.
				for (int index = 0; index < targets.Length; index++) // foreach (IxCoreColleague host in targets) 
				{
					if (!ProcessMessages)
						return true;

					IxCoreColleague host = targets[index];
					//colleagues can be removed when something (like the window) handles this event
					//so make sure this guy is still legit!
					if (!m_colleagues.Contains(host))
						continue;

					if (invokeSwitch.TraceVerbose)
						Trace.WriteLine("[" + host.ToString(), invokeSwitch.DisplayName);

					Trace.Indent();
					handled = InvokeRecursively(host, methodName, parameterTypes, parameterList, previous, stopWhenHandled, justCheckingForReceivers);

					if (!ProcessMessages)
						return true;

					Trace.Unindent();

					if (invokeSwitch.TraceVerbose)
					{
						Trace.WriteLine("host:" + host.ToString() + " handled methodName=" + handled, invokeSwitch.DisplayName);
						Trace.WriteLine("]", invokeSwitch.DisplayName);
					}

					if (handled)
					{
						m_MethodsOnAnyColleague.Add(methodName);
						if (stopWhenHandled)
						{
							Trace.WriteLineIf(invokeSwitch.TraceVerbose, "-->handled=true And stopWhenHandled=true", invokeSwitch.DisplayName);
							break;
						}
						else
						{
							Trace.WriteLineIf(invokeSwitch.TraceVerbose, "-->handled=true", invokeSwitch.DisplayName);
						}
					}
				}
			}
			finally
			{
				if (invokeSwitch.TraceVerbose)
					Trace.Unindent();
			}
			return handled;
		}

		static long m_invokeCount = 0;
		
		//static System.Diagnostics.Stopwatch ttime = new Stopwatch();
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Invokes the recursively.
		/// </summary>
		/// <param name="colleague">The colleague.</param>
		/// <param name="methodName">Name of the method.</param>
		/// <param name="parameterTypes">The parameter types.</param>
		/// <param name="parameterList">The parameter list.</param>
		/// <param name="previous">to catch infinite loops</param>
		/// <param name="stopWhenHandled">if set to <c>true</c> [stop when handled].</param>
		/// <param name="justCheckingForReceivers">if set to <c>true</c> [just checking for receivers].</param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		private bool InvokeRecursively(IxCoreColleague colleague, string methodName, Type[] parameterTypes,
			object[] parameterList, HashSet<int> previous, bool stopWhenHandled, bool justCheckingForReceivers)
		{

			if (!ProcessMessages)
				return true;

			bool handled = false;
//			Trace.Indent();
			if (invokeSwitch.TraceVerbose)
			{
				Trace.WriteLine("InvokeRecursively: methodName=<" + methodName + "> colleague=<" + colleague.ToString() + ">", invokeSwitch.DisplayName);
			}
			m_invokeCount++;
			//////// THIS IS TESTING CODE ADDED AND COMMENTED AND NEEDS TO BE REVERTED WHEN DONE>>>>>>>>
			//////// ><<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<>
			////////ttime.Start();
			bool chbDisposed = ColleagueHasBeenDisposed(colleague);
			////////ttime.Stop();
			////////if ((m_invokeCount % 1000) == 0)
			////////{
			////////    TimeSpan ts = ttime.Elapsed;
			////////    string tsString = String.Format("{0:00}.{1:0000}({2})",
			////////        ts.Seconds,	ts.Milliseconds, ts.Ticks );
			////////    Trace.WriteLine(tsString + " ColleagueHasBeenDisposed(" + colleague.ToString() + ")="+chbDisposed.ToString());
			////////    ttime.Reset();
			////////}
			if (chbDisposed)
			{
				DebugMsg("_+_+_+_+ InvokeRecursively called on colleague that is disposed/disposing: " + colleague.ToString());
				return false;	// stop the processing
			}

			IxCoreColleague[] targets = colleague.GetMessageTargets();
			// Try following the 'Code Performance' guidelines which says that
			// .."foreach introduces both managed heap and virtual function overhead..
			// This can be a significant factor in performance-sensitive regions of your application."
			// This section of code is indeed a very performance-sensitive region!
			//
			for (int index = 0; index < targets.Length; index++) // foreach(IxCoreColleague target in targets)
			{
				if (!ProcessMessages)
					return true;

				IxCoreColleague target = targets[index];
				if(target == null)
				{
					//Debug.WriteLine("Warning, target null.");
					continue;
				}
				
				//this section is the leaf of the search tree
				if (target == colleague)
				{
					if (previous.Contains(target.GetHashCode()))
					{
						//we have already encountered this target once.
						//how can we encounter the same one twice?
						//This will happen when more than one colleague includes some shared object as one of its children.
						//in xWorks, this happens with the RecordClerk, which is not a top-level colleague on its own.
						break;
					}
					if (IsMethodNOTonColleague(methodName, colleague.ToString()) == false)	// prefilter using history
					{
						MethodInfo mi = CheckForMatchingMessage(colleague, methodName, parameterTypes, parameterList, previous);
						if (mi!=null)
						{
							if(justCheckingForReceivers)
							{
								handled= true;
								break;
							}
							else
							{
								object o = InvokeMethod( target,  mi, parameterList);
								handled = (o!=null)?(bool)o : false;
							}
						}
						else
						{
							AddMethodNOTonColleague(methodName, colleague.ToString());
						}
					}
					else
					{
						m_SavedCalls++;
					}
				}
				else //not at a leaf yet, keep going down the tree
					handled = InvokeRecursively(target, methodName,  parameterTypes, parameterList, previous, stopWhenHandled, justCheckingForReceivers);

				if(handled && stopWhenHandled)
				{
					Trace.WriteLineIf(invokeSwitch.TraceVerbose, "-->handled=true And stopWhenHandled=true", invokeSwitch.DisplayName);
					break;
				}
				else if(handled)
					Trace.WriteLineIf(invokeSwitch.TraceVerbose, "-->handled=true", invokeSwitch.DisplayName);

			}
//			TraceVerboseLine("}");
//			Trace.Unindent();
			return handled;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Colleagues the has been disposed.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		private bool ColleagueHasBeenDisposed(IxCoreColleague target)
		{
			bool hasBeenDisposed = false;

			if (target is IDisposable)
			{
				// Have to use reflection to see if it has an IsDisposed property,
				// since that is not part of the interface for IDisposable for some odd reason.
				// Get the property infor for the ActiveViewHelper property on the control
				// (assuming there is one).
				PropertyInfo propInfo = target.GetType().GetProperty("IsDisposed",
					BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

				// If we successfully found an ActiveViewHelper property, then reference
				// it to set our class' active view helper member variable.
				if (propInfo != null)
				{
					hasBeenDisposed = (bool)propInfo.GetValue(target,
						BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
						null, null, null);

					if (!hasBeenDisposed)
					{
						propInfo = target.GetType().GetProperty("Disposing",
							BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

						// If we successfully found an ActiveViewHelper property, then reference
						// it to set our class' active view helper member variable.
						if (propInfo != null)
						{
							hasBeenDisposed = (bool)propInfo.GetValue(target,
								BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
								null, null, null);
						}
					}
				}
			}

			return hasBeenDisposed;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Checks for matching message.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="methodName">Name of the method.</param>
		/// <param name="parameterTypes">Currently, use null here if you have a ref param</param>
		/// <param name="parameterList">The parameter list.</param>
		/// <param name="previous">The previous.</param>
		/// <returns>
		/// null or the MethodInfo if a matching one was found
		/// </returns>
		/// ------------------------------------------------------------------------------------
		private MethodInfo CheckForMatchingMessage(IxCoreColleague target, string methodName, Type[] parameterTypes,
			object[] parameterList, HashSet<int> previous)
		{
#if false
			//tostring here is too expensive to leave lying around
			Trace.Indent();
			TraceVerboseLine(" Checking : "+ target.ToString());
#endif
			int x = target.GetHashCode();
			if (previous.Contains(x))
			{
				throw new ArgumentException("XCore Mediator encountered the same " + target.ToString() + " twice on check for " + methodName + ", as if there is a loop.");
			}
#if false
			TraceVerboseLine("Adding "+target.ToString()+":"+x.ToString());
#endif
			previous.Add(x);

			//NB: I did not think about these flags; I just copied them from an example
			BindingFlags flags =
				BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance;

			Type type = target.GetType();
			MethodInfo mi;
			if (parameterTypes == null) // callers currently must use null here if they have a "ref" param (TODO)
			{
				mi=type.GetMethod(methodName,flags);
			}
			else
			{
				mi= type.GetMethod(methodName, flags, null,	parameterTypes, null);
			}
#if false
			Trace.Unindent();
#endif
			return mi;
		}

		/// <summary>
		/// Exception that is a special case where the Mediator is working,
		/// but due to re-entrance has been disposed of in a stack frame that
		/// is no longer active, but as a result we are in a disposed of state
		/// and should go straight to the end of our current call frame: SendMessage
		/// </summary>
		class DisposedInAnotherFrameException : Exception {}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Invokes the method.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="mi">The mi.</param>
		/// <param name="parameterList">The parameter list.</param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		private object InvokeMethod(IxCoreColleague target, MethodInfo mi, object[] parameterList)
		{
			if (!ProcessMessages)
				return null;

			try
			{
				Trace.Indent();

				if (invokeSwitch.TraceInfo)
				{
					if (parameterList.Length > 0 && parameterList[0] != null)
					{
						Trace.WriteLine(" Invoking Method: " + mi.Name +
							"('" + parameterList[0].ToString() + "')" +
							" on " + target.ToString(), invokeSwitch.DisplayName);
					}
					else
					{
						Trace.WriteLine(" Invoking Method: " + mi.Name +
						" on " + target.ToString(), invokeSwitch.DisplayName);
					}
				}
#if false
				string objName = "";
				objName = target.ToString() + target.GetHashCode().ToString();
				// DLH TESTING - not finished yet...
				if (IsDisposedColleague(objName))
				{
				    Debug.WriteLine("##Not Invoking disposed object:"+objName);
				    return null;
				}
#endif
				// *****************************************************************************
				// Have seen a stack situation that the Mediator has been disposed of after 
				// returning from this call - IOW's the following call allows re-entrance.  
				// That's why the exception follows to handle a known case when processing the
				// ExitApplication msg.
				// *****************************************************************************
				object returnValue = mi.Invoke(target, parameterList);
				if (m_isDisposed)
					throw new DisposedInAnotherFrameException();

				if (target == m_temporaryColleague && !mi.Name.StartsWith("OnDisplay"))
				{
					RemoveColleague(m_temporaryColleague);
					m_temporaryColleague = null;	// only keep one temporary colleague at a time (menu based)
				}

				Trace.Unindent();

				return returnValue;
			}

			//Note that we don't want to catch just any kind of the exception here,
			//most exceptions will be invoked by the method that we actually called.
			//the only exceptions we want to catch are the ones that just mean that we failed to find
			//a suitable method. These we can report if in debug-mode, otherwise ignore.
			catch(ArgumentException error)
			{
				//I once spent close to an hour wondering what was causing the failure here.
				//The exception message was "Object type cannot be converted to target type."
				//the answer was that I had made the signature be a UIListDisplayProperties
				//when it should have been a UIItemDisplayProperties. The two can be pretty hard to
				//distinguish visually. (John Hatton)
				Debug.Fail("The method '"+mi.Name+"' was found but couldn't be invoked. Maybe has the wrong signature?", error.Message);
			}
			catch(TargetInvocationException error)
			{
				Exception inner = error.InnerException;	//unwrap, for example, a ConfigurationException
				// See LT-1629 "Closing one db while opening another crashes".  The following
				// two lines  appear to fix this bug, although I'm not too happy about this
				// asynchronous behavior appearing where we (or at least I) don't expect it.
				// Unfortunately, that's inherent with message handling architecture when
				// handling one message allows other messages to be handled before it finishes.
				// - SteveMc
				if (inner is NullReferenceException &&
					mi.Name == "OnChooseLanguageProject")
				{
					// We probably closed the target's window after choosing another project,
					// but before getting to this point in processing the ChooseLP message.  So
					// ignore the exception.
					return null;
				}
				string s = "Something went wrong trying to invoke "+ target.ToString() +":"+ mi.Name +"().";
				//if we just send on the error, then the caller
				//will find it more easy to trap particular kind of exceptions. On the other hand,
				//if the exception makes it all the way to the user, then we will really want to see this extra string (s)
				//at the top level.

				throw new Exception(s, inner);

				//if we were to just bring up the blue box), then that makes it impossible for the caller to catch
				//the exception. In particular, the link-jumping column can fail if the object it is trying to jump to
				//has been deleted. This is really not and "error", and if we let the exception get back to the
				//jumping code, then it can notify the user more calmly.
				//SIL.Utils.ErrorReporter.ReportException(new ApplicationException(s, inner));
			}
			return null;
		}

		#endregion

		#region Other methods
		public string GetRealPath(string path)
		{
			CheckDisposed();
			foreach(KeyValuePair<string, string> kvp in  m_pathVariables)
			{
				string key = kvp.Key;
				if (path.Contains(key))
				{
					path = path.Replace(key, kvp.Value);
					break;
				}
			}
			return path;
		}
		#endregion

		#region Colleague handling methods

		public string GetColleaguesDumpString()
		{
			CheckDisposed();

			StringBuilder sb = new StringBuilder("");
			foreach(IxCoreColleague icc in m_colleagues)
			{
				sb.AppendFormat("{0}\r\n", icc.ToString());
			}
			return sb.ToString();
		}

		public void AddTemporaryColleague(IxCoreColleague colleague)
		{
			CheckDisposed();

			if (m_temporaryColleague != null)
				RemoveColleague(m_temporaryColleague);
			
			m_temporaryColleague = colleague;	// only keep one temporary colleague at a time (menu based)
			AddColleague(m_temporaryColleague);
		}

		public void AddColleague(IxCoreColleague colleague)
		{
			CheckDisposed();

			// Note: m_colleagues is now a Set, so would ignore the attempt to add it again.
			// The problem with that is it is really a programming error to add them more than once.
			// So, we will keep the exception.
			if (m_colleagues.Contains(colleague))
				throw new ApplicationException ("This object is already in the list of colleagues.");

			m_colleagues.Add(colleague);
		}

		public void RemoveColleague(IxCoreColleague colleague)
		{
			CheckDisposed();
			// No need to check if m_colleagues is null.
			// if it hasn't been dispoded, as it will always be non-null.
			// If it has been disposed, then the caller should be fixed to be better
			// behaved about calling disposed objects.  (Read: it shouldn't ever call code on disposed objects.)
			//if (m_colleagues != null)
			m_colleagues.Remove(colleague);
		}
		#endregion

		#region Trace and Log methods
#if false
		private void TraceVerboseLine(string s)
		{
			if (m_traceSwitch != null && m_traceSwitch.TraceVerbose)
				Trace.WriteLine("MID="+System.Threading.Thread.CurrentThread.GetHashCode()+": "+s);
		}
		private void TraceInfoLine(string s)
		{
			if (m_traceSwitch != null && m_traceSwitch.TraceInfo || m_traceSwitch.TraceVerbose)
				Trace.WriteLine("MID="+System.Threading.Thread.CurrentThread.GetHashCode()+": "+s);
		}	
		public TraceLevel TraceLevel
		{
			set
			{
				CheckDisposed();
				m_traceSwitch.Level = value;
			}
			get
			{
				CheckDisposed();
				return m_traceSwitch.Level;
			}
		}
#endif
		public void Log(string message)
		{
			CheckDisposed();
			//Logger.WriteEvent(message);
		}
		#endregion
	}
}
