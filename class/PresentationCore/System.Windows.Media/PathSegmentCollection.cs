
/* this file is generated by gen-collection-types.cs.  do not modify */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Media.Animation;

namespace System.Windows.Media {


	public class PathSegmentCollection : Animatable, ICollection<PathSegment>, IList<PathSegment>, ICollection, IList, IFormattable
	{
		List<PathSegment> list;

		public struct Enumerator : IEnumerator<PathSegment>, IEnumerator
		{
			public void Reset()
			{
				throw new NotImplementedException (); 
			}

			public bool MoveNext()
			{
				throw new NotImplementedException (); 
			}

			public PathSegment Current {
				get { throw new NotImplementedException (); }
			}

			object IEnumerator.Current {
				get { return Current; }
			}

			void IDisposable.Dispose()
			{
			}
		}

		public PathSegmentCollection ()
		{
			list = new List<PathSegment>();
		}

		public PathSegmentCollection (IEnumerable<PathSegment> values)
		{
			list = new List<PathSegment> (values);
		}

		public PathSegmentCollection (int length)
		{
			list = new List<PathSegment> (length);
		}

		public new PathSegmentCollection Clone ()
		{
			throw new NotImplementedException ();
		}

		public new PathSegmentCollection CloneCurrentValue ()
		{
			throw new NotImplementedException ();
		}

		public bool Contains (PathSegment value)
		{
			return list.Contains (value);
		}

		public bool Remove (PathSegment value)
		{
			return list.Remove (value);
		}

		public int IndexOf (PathSegment value)
		{
			return list.IndexOf (value);
		}

		public void Add (PathSegment value)
		{
			list.Add (value);
		}

		public void Clear ()
		{
			list.Clear ();
		}

		public void CopyTo (PathSegment[] array, int arrayIndex)
		{
			list.CopyTo (array, arrayIndex);
		}

		public void Insert (int index, PathSegment value)
		{
			list.Insert (index, value);
		}

		public void RemoveAt (int index)
		{
			list.RemoveAt (index);
		}

		public int Count {
			get { return list.Count; }
		}

		public PathSegment this[int index] {
			get { return list[index]; }
			set { list[index] = value; }
		}

		public static PathSegmentCollection Parse (string str)
		{
			throw new NotImplementedException ();
		}

		bool ICollection<PathSegment>.IsReadOnly {
			get { return false; }
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator();
		}

		IEnumerator<PathSegment> IEnumerable<PathSegment>.GetEnumerator()
		{
			return GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator();
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		object ICollection.SyncRoot {
			get { return this; }
		}

		void ICollection.CopyTo(Array array, int offset)
		{
			CopyTo ((PathSegment[]) array, offset);
		}

		bool IList.IsFixedSize {
			get { return false; }
		}

		bool IList.IsReadOnly {
			get { return false; }
		}

		object IList.this[int index] {
			get { return this[index]; }
			set { this[index] = (PathSegment)value; }
		}

		int IList.Add (object value)
		{
			Add ((PathSegment)value);
			return Count;
		}

		bool IList.Contains (object value)
		{
			return Contains ((PathSegment)value);
		}

		int IList.IndexOf (object value)
		{
			return IndexOf ((PathSegment)value);
		}

		void IList.Insert (int index, object value)
		{
			Insert (index, (PathSegment)value);
		}

		void IList.Remove (object value)
		{
			Remove ((PathSegment)value);
		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		public string ToString (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		string IFormattable.ToString (string format, IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}


		protected override bool FreezeCore (bool isChecking)
		{{
			if (isChecking) {{
				return base.FreezeCore (isChecking);
			}}
			else {{
				return true;
			}}
		}}



		protected override Freezable CreateInstanceCore ()
		{
			return new PathSegmentCollection();
		}

		protected override void GetAsFrozenCore (Freezable sourceFreezable)
		{
			throw new NotImplementedException ();
		}

		protected override void GetCurrentValueAsFrozenCore (Freezable sourceFreezable)
		{
			throw new NotImplementedException ();
		}

		protected override void CloneCurrentValueCore (Freezable sourceFreezable)
		{
			throw new NotImplementedException ();
		}

		protected override void CloneCore (Freezable sourceFreezable)
		{
			throw new NotImplementedException ();
		}
	}
}