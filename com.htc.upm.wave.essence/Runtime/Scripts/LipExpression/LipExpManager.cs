// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC\u2019s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System;
using System.Threading;
using UnityEngine;
using Wave.Native;
using Wave.OpenXR;

namespace Wave.Essence.LipExpression
{
	public class LipExpManager : MonoBehaviour
	{
		private const string LOG_TAG = "Wave.Essence.LipExp.LipExpManager";
		private static void DEBUG(string msg)
		{
			if (Log.EnableDebugLog)
				Log.d(LOG_TAG, msg, true);
		}
		private static void INFO(string msg) { Log.i(LOG_TAG, msg, true); }

		public enum LipExpStatus
		{
			// Initial, can call Start API in this state.
			NotStart = 0,
			StartFailure,

			// Processing, should NOT call API in this state.
			Starting,
			Stopping,

			// Running, can call Stop API in this state.
			Available,

			// Do nothing.
			NoSupport
		}

		#region Inspector
		[SerializeField]
		private bool m_InitialStart = false;
		public bool InitialStart { get { return m_InitialStart; } set { m_InitialStart = value; } }

		[SerializeField]
		private bool m_UseXRDevice = false;
		public bool UseXRDevice { get { return m_UseXRDevice; } set { m_UseXRDevice = value; } }
		private bool UseXRData()
		{
			return m_UseXRDevice && !Application.isEditor;
		}
		#endregion

		private static LipExpManager m_Instance = null;
		public static LipExpManager Instance { get { return m_Instance; } }

		#region MonoBehaviour overrides
		private void Awake()
		{
			m_Instance = this;

			var supportedFeature = Interop.WVR_GetSupportedFeatures();
			INFO("Awake() supportedFeature: " + supportedFeature);

			if ((supportedFeature & (ulong)WVR_SupportedFeature.WVR_SupportedFeature_LipExp) == 0)
			{
				Log.w(LOG_TAG, "WVR_SupportedFeature_LipExpression is not enabled.", true);
				SetLipExpStatus(LipExpStatus.NoSupport);
			}
		}
		bool mEnabled = false;
		private void OnEnable()
		{
			if (!mEnabled)
			{
				INFO("OnEnable()");
				if (m_InitialStart) { StartLipExp(); }

				mEnabled = true;
			}
		}
		private void OnDisable()
		{
			if (mEnabled)
			{
				INFO("OnDisable()");
				mEnabled = false;
			}
		}
		private void Update()
		{
			UpdateData();
		}
		#endregion

		#region Life Cycle
		private LipExpStatus m_LipExpStatus = LipExpStatus.NotStart;
		private static ReaderWriterLockSlim m_LipExpStatusRWLock = new ReaderWriterLockSlim();
		private void SetLipExpStatus(LipExpStatus status)
		{
			try
			{
				m_LipExpStatusRWLock.TryEnterWriteLock(2000);
				m_LipExpStatus = status;
			}
			catch (Exception e)
			{
				Log.e(LOG_TAG, "SetLipExpStatus() " + e.Message, true);
				throw;
			}
			finally
			{
				m_LipExpStatusRWLock.ExitWriteLock();
			}
		}

		private bool CanStartLipExp()
		{
			LipExpStatus status = GetLipExpStatus();
			if (status == LipExpStatus.NotStart ||
				status == LipExpStatus.StartFailure)
			{
				return true;
			}
			return false;
		}
		private bool CanStopLipExp()
		{
			LipExpStatus status = GetLipExpStatus();
			if (status == LipExpStatus.Available)
			{
				return true;
			}
			return false;
		}

		public delegate void LipExpResultDelegate(object sender, bool result);
		private event LipExpResultDelegate lipExpResultCB = null;
		private void StartLipExpLock()
		{
			if (UseXRData())
			{
				InputDeviceLip.ActivateLipExp(true);
				return;
			}

			if (!CanStartLipExp()) { return; }

			SetLipExpStatus(LipExpStatus.Starting);
			WVR_Result result = Interop.WVR_StartLipExp();
			switch (result)
			{
				case WVR_Result.WVR_Success:
					SetLipExpStatus(LipExpStatus.Available);
					break;
				case WVR_Result.WVR_Error_FeatureNotSupport:
					SetLipExpStatus(LipExpStatus.NoSupport);
					break;
				default:
					SetLipExpStatus(LipExpStatus.StartFailure);
					break;
			}
			DEBUG("StartLipExpLock() result: " + result);

			if (lipExpResultCB != null)
			{
				lipExpResultCB(this, result == WVR_Result.WVR_Success ? true : false);
				lipExpResultCB = null;
			}
		}

		private object lipExpThreadLocker = new object();
		private void StartLipExpThread()
		{
			lock (lipExpThreadLocker)
			{
				DEBUG("StartLipExpThread()");
				StartLipExpLock();
			}
		}

		private void StopLipExpLock()
		{
			if (UseXRData())
			{
				InputDeviceLip.ActivateLipExp(false);
				return;
			}

			if (!CanStopLipExp()) { return; }

			SetLipExpStatus(LipExpStatus.Stopping);
			INFO("StopLipExpLock()");
			Interop.WVR_StopLipExp();
			SetLipExpStatus(LipExpStatus.NotStart);
		}
		private void StopLipExpThread()
		{
			lock (lipExpThreadLocker)
			{
				DEBUG("StopLipExpThread()");
				StopLipExpLock();
			}
		}
		#endregion

		#region Lip Expression Data
		private float[] m_LipExpValues = new float[(uint)LipExp.Max];
		private bool hasLipExpData = false;
		void UpdateData()
		{
			var status = GetLipExpStatus();
			if (status == LipExpStatus.Available)
			{
				var result = Interop.WVR_GetLipExpData(m_LipExpValues);
				hasLipExpData = (result == WVR_Result.WVR_Success);
			}
			else
			{
				hasLipExpData = false;
			}
		}
		#endregion

		#region Public Functions
		public LipExpStatus GetLipExpStatus()
		{
			if (UseXRData())
			{
				uint statusId = InputDeviceLip.GetLipExpStatus();
				LipExpStatus status = LipExpStatus.NoSupport;

				if (statusId == 0) { status = LipExpStatus.NotStart; }
				if (statusId == 1) { status = LipExpStatus.StartFailure; }
				if (statusId == 2) { status = LipExpStatus.Starting; }
				if (statusId == 3) { status = LipExpStatus.Stopping; }
				if (statusId == 4) { status = LipExpStatus.Available; }
				if (statusId == 5) { status = LipExpStatus.NoSupport; }

				return status;
			}

			try
			{
				m_LipExpStatusRWLock.TryEnterReadLock(2000);
				return m_LipExpStatus;
			}
			catch (Exception e)
			{
				Log.e(LOG_TAG, "GetLipExpStatus() " + e.Message, true);
				throw;
			}
			finally
			{
				m_LipExpStatusRWLock.ExitReadLock();
			}
		}

		private uint m_LipExpRefCount = 0;
		public void StartLipExp(LipExpResultDelegate callback)
		{
			if (lipExpResultCB == null)
			{
				lipExpResultCB = callback;
			}
			else
			{
				lipExpResultCB += callback;
			}

			StartLipExp();
		}
		public void StartLipExp()
		{
			//string caller = new StackFrame(1, true).GetMethod().Name;
			m_LipExpRefCount++;
			//Log.i(LOG_TAG, "StartLipExp(" + m_LipExpRefCount + ") from " + caller, true);

			if (!CanStartLipExp())
			{
				DEBUG("StartLipExp() can NOT start lip expression.");
				if (lipExpResultCB != null) { lipExpResultCB = null; }
				return;
			}

			Thread lipExp_t = new Thread(StartLipExpThread);
			lipExp_t.Name = "StartLipExpThread";
			lipExp_t.Start();
		}
		public void StopLipExp()
		{
			//string caller = new StackFrame(1, true).GetMethod().Name;
			m_LipExpRefCount--;
			//Log.i(LOG_TAG, "StopLipExp(" + m_LipExpRefCount + ") from " + caller, true);
			if (m_LipExpRefCount > 0) { return; }

			if (!CanStopLipExp())
			{
				DEBUG("CanStopLipExp() can NOT stop lip expression.");
				return;
			}

			Thread tracker_t = new Thread(StopLipExpThread);
			tracker_t.Name = "StopLipExpThread";
			tracker_t.Start();
		}

		public bool IsLipExpEnabled()
		{
			var status = GetLipExpStatus();
			return (status == LipExpStatus.Available);
		}

		public float GetLipExpression(LipExp lipExp)
		{
			if (hasLipExpData && (int)lipExp >= 0 && (int)lipExp < (int)LipExp.Max)
			{
				return m_LipExpValues[(uint)lipExp];
			}
			return 0;
		}
		public bool GetLipExpressions(out float[] lipExps)
		{
			lipExps = m_LipExpValues;
			return hasLipExpData;
		}
		#endregion
	}
}