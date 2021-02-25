using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedInterpol("{\"inter\":[0.15,0.15]")]
	public partial class PlayerNetworkObject : NetworkObject
	{
		public const int IDENTITY = 8;

		private byte[] _dirtyFields = new byte[1];

		#pragma warning disable 0067
		public event FieldChangedEvent fieldAltered;
		#pragma warning restore 0067
		[ForgeGeneratedField]
		private Vector3 _pos;
		public event FieldEvent<Vector3> posChanged;
		public InterpolateVector3 posInterpolation = new InterpolateVector3() { LerpT = 0.15f, Enabled = true };
		public Vector3 pos
		{
			get { return _pos; }
			set
			{
				// Don't do anything if the value is the same
				if (_pos == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x1;
				_pos = value;
				hasDirtyFields = true;
			}
		}

		public void SetposDirty()
		{
			_dirtyFields[0] |= 0x1;
			hasDirtyFields = true;
		}

		private void RunChange_pos(ulong timestep)
		{
			if (posChanged != null) posChanged(_pos, timestep);
			if (fieldAltered != null) fieldAltered("pos", _pos, timestep);
		}
		[ForgeGeneratedField]
		private Quaternion _rot;
		public event FieldEvent<Quaternion> rotChanged;
		public InterpolateQuaternion rotInterpolation = new InterpolateQuaternion() { LerpT = 0.15f, Enabled = true };
		public Quaternion rot
		{
			get { return _rot; }
			set
			{
				// Don't do anything if the value is the same
				if (_rot == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x2;
				_rot = value;
				hasDirtyFields = true;
			}
		}

		public void SetrotDirty()
		{
			_dirtyFields[0] |= 0x2;
			hasDirtyFields = true;
		}

		private void RunChange_rot(ulong timestep)
		{
			if (rotChanged != null) rotChanged(_rot, timestep);
			if (fieldAltered != null) fieldAltered("rot", _rot, timestep);
		}

		protected override void OwnershipChanged()
		{
			base.OwnershipChanged();
			SnapInterpolations();
		}
		
		public void SnapInterpolations()
		{
			posInterpolation.current = posInterpolation.target;
			rotInterpolation.current = rotInterpolation.target;
		}

		public override int UniqueIdentity { get { return IDENTITY; } }

		protected override BMSByte WritePayload(BMSByte data)
		{
			UnityObjectMapper.Instance.MapBytes(data, _pos);
			UnityObjectMapper.Instance.MapBytes(data, _rot);

			return data;
		}

		protected override void ReadPayload(BMSByte payload, ulong timestep)
		{
			_pos = UnityObjectMapper.Instance.Map<Vector3>(payload);
			posInterpolation.current = _pos;
			posInterpolation.target = _pos;
			RunChange_pos(timestep);
			_rot = UnityObjectMapper.Instance.Map<Quaternion>(payload);
			rotInterpolation.current = _rot;
			rotInterpolation.target = _rot;
			RunChange_rot(timestep);
		}

		protected override BMSByte SerializeDirtyFields()
		{
			dirtyFieldsData.Clear();
			dirtyFieldsData.Append(_dirtyFields);

			if ((0x1 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _pos);
			if ((0x2 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _rot);

			// Reset all the dirty fields
			for (int i = 0; i < _dirtyFields.Length; i++)
				_dirtyFields[i] = 0;

			return dirtyFieldsData;
		}

		protected override void ReadDirtyFields(BMSByte data, ulong timestep)
		{
			if (readDirtyFlags == null)
				Initialize();

			Buffer.BlockCopy(data.byteArr, data.StartIndex(), readDirtyFlags, 0, readDirtyFlags.Length);
			data.MoveStartIndex(readDirtyFlags.Length);

			if ((0x1 & readDirtyFlags[0]) != 0)
			{
				if (posInterpolation.Enabled)
				{
					posInterpolation.target = UnityObjectMapper.Instance.Map<Vector3>(data);
					posInterpolation.Timestep = timestep;
				}
				else
				{
					_pos = UnityObjectMapper.Instance.Map<Vector3>(data);
					RunChange_pos(timestep);
				}
			}
			if ((0x2 & readDirtyFlags[0]) != 0)
			{
				if (rotInterpolation.Enabled)
				{
					rotInterpolation.target = UnityObjectMapper.Instance.Map<Quaternion>(data);
					rotInterpolation.Timestep = timestep;
				}
				else
				{
					_rot = UnityObjectMapper.Instance.Map<Quaternion>(data);
					RunChange_rot(timestep);
				}
			}
		}

		public override void InterpolateUpdate()
		{
			if (IsOwner)
				return;

			if (posInterpolation.Enabled && !posInterpolation.current.UnityNear(posInterpolation.target, 0.0015f))
			{
				_pos = (Vector3)posInterpolation.Interpolate();
				//RunChange_pos(posInterpolation.Timestep);
			}
			if (rotInterpolation.Enabled && !rotInterpolation.current.UnityNear(rotInterpolation.target, 0.0015f))
			{
				_rot = (Quaternion)rotInterpolation.Interpolate();
				//RunChange_rot(rotInterpolation.Timestep);
			}
		}

		private void Initialize()
		{
			if (readDirtyFlags == null)
				readDirtyFlags = new byte[1];

		}

		public PlayerNetworkObject() : base() { Initialize(); }
		public PlayerNetworkObject(NetWorker networker, INetworkBehavior networkBehavior = null, int createCode = 0, byte[] metadata = null) : base(networker, networkBehavior, createCode, metadata) { Initialize(); }
		public PlayerNetworkObject(NetWorker networker, uint serverId, FrameStream frame) : base(networker, serverId, frame) { Initialize(); }

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}
