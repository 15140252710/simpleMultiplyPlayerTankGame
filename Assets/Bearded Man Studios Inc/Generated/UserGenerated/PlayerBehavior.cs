using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedRPC("{\"types\":[[\"uint\", \"Color\"][\"string\"][\"uint\"][\"uint\"][\"uint\", \"int\"][]]")]
	[GeneratedRPCVariableNames("{\"types\":[[\"ID\", \"Color\"][\"OwnerName\"][\"ID\"][\"shooterID\"][\"shooterID\", \"damage\"][]]")]
	public abstract partial class PlayerBehavior : NetworkBehavior
	{
		public const byte RPC_SET_BODY_COLOR = 0 + 5;
		public const byte RPC_SET_OWNER_NAME = 1 + 5;
		public const byte RPC_SET_NETWORK_I_D_SYNC = 2 + 5;
		public const byte RPC_FIRE_SHELL = 3 + 5;
		public const byte RPC_GET_DAMAGE_SYNC = 4 + 5;
		public const byte RPC_DEAD_SYNC = 5 + 5;
		
		public PlayerNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (PlayerNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("SetBodyColor", SetBodyColor, typeof(uint), typeof(Color));
			networkObject.RegisterRpc("SetOwnerName", SetOwnerName, typeof(string));
			networkObject.RegisterRpc("SetNetworkIDSync", SetNetworkIDSync, typeof(uint));
			networkObject.RegisterRpc("FireShell", FireShell, typeof(uint));
			networkObject.RegisterRpc("GetDamageSync", GetDamageSync, typeof(uint), typeof(int));
			networkObject.RegisterRpc("DeadSync", DeadSync);

			networkObject.onDestroy += DestroyGameObject;

			if (!obj.IsOwner)
			{
				if (!skipAttachIds.ContainsKey(obj.NetworkId)){
					uint newId = obj.NetworkId + 1;
					ProcessOthers(gameObject.transform, ref newId);
				}
				else
					skipAttachIds.Remove(obj.NetworkId);
			}

			if (obj.Metadata != null)
			{
				byte transformFlags = obj.Metadata[0];

				if (transformFlags != 0)
				{
					BMSByte metadataTransform = new BMSByte();
					metadataTransform.Clone(obj.Metadata);
					metadataTransform.MoveStartIndex(1);

					if ((transformFlags & 0x01) != 0 && (transformFlags & 0x02) != 0)
					{
						MainThreadManager.Run(() =>
						{
							transform.position = ObjectMapper.Instance.Map<Vector3>(metadataTransform);
							transform.rotation = ObjectMapper.Instance.Map<Quaternion>(metadataTransform);
						});
					}
					else if ((transformFlags & 0x01) != 0)
					{
						MainThreadManager.Run(() => { transform.position = ObjectMapper.Instance.Map<Vector3>(metadataTransform); });
					}
					else if ((transformFlags & 0x02) != 0)
					{
						MainThreadManager.Run(() => { transform.rotation = ObjectMapper.Instance.Map<Quaternion>(metadataTransform); });
					}
				}
			}

			MainThreadManager.Run(() =>
			{
				NetworkStart();
				networkObject.Networker.FlushCreateActions(networkObject);
			});
		}

		protected override void CompleteRegistration()
		{
			base.CompleteRegistration();
			networkObject.ReleaseCreateBuffer();
		}

		public override void Initialize(NetWorker networker, byte[] metadata = null)
		{
			Initialize(new PlayerNetworkObject(networker, createCode: TempAttachCode, metadata: metadata));
		}

		private void DestroyGameObject(NetWorker sender)
		{
			MainThreadManager.Run(() => { try { Destroy(gameObject); } catch { } });
			networkObject.onDestroy -= DestroyGameObject;
		}

		public override NetworkObject CreateNetworkObject(NetWorker networker, int createCode, byte[] metadata = null)
		{
			return new PlayerNetworkObject(networker, this, createCode, metadata);
		}

		protected override void InitializedTransform()
		{
			networkObject.SnapInterpolations();
		}

		/// <summary>
		/// Arguments:
		/// uint ID
		/// Color Color
		/// </summary>
		public abstract void SetBodyColor(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// string OwnerName
		/// </summary>
		public abstract void SetOwnerName(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// uint ID
		/// </summary>
		public abstract void SetNetworkIDSync(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// uint shooterID
		/// </summary>
		public abstract void FireShell(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// uint shooterID
		/// int damage
		/// </summary>
		public abstract void GetDamageSync(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void DeadSync(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}