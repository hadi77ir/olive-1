using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Activities;
using System.Windows.Markup;

namespace System.Activities.Tracking
{
	[DataContract]
	public sealed class WorkflowInstanceAbortedRecord : WorkflowInstanceRecord
	{
		[MonoTODO ("verify state")]
		public WorkflowInstanceAbortedRecord (Guid instanceId, string activityDefinitionId, string reason)
			: base (instanceId, activityDefinitionId, null)
		{
			Reason = reason;
		}

		[MonoTODO ("verify state")]
		public WorkflowInstanceAbortedRecord (Guid instanceId, long recordNumber, string activityDefinitionId, string reason)
			: base (instanceId, recordNumber, activityDefinitionId, null)
		{
			Reason = reason;
		}

		[DataMember]
		public string Reason { get; private set; }

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
	}
}
