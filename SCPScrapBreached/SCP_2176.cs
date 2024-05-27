using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace SCPScrapBreached
{
    public class SCP_2176 : GrabbableObject
    {
        public float fLightKillRange = 15f;

        // Cause all nearby lights to flicker off
        public override void PlayDropSFX() {
            AudioSource component = gameObject.GetComponent<AudioSource>();
            component.PlayOneShot(itemProperties.dropSFX);
            WalkieTalkie.TransmitOneShotAudio(component, itemProperties.dropSFX);

            if (IsOwner) {
                RoundManager.Instance.PlayAudibleNoise(transform.position, 8f, 0.5f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed, 941);
                KillNearbyLightsServerRpc();
            }
            hasHitGround = true;
        }

        private IEnumerator KillNearbyLightsCoroutine() {
            Vector3 vDropPosition = transform.position;
            List<Animator> lNearbvLights = RoundManager.Instance.allPoweredLightsAnimators.FindAll(x => Vector3.Distance(vDropPosition, x.transform.position) < fLightKillRange);
            foreach (Animator light in lNearbvLights) {
                light.SetTrigger("Flicker");
                yield return new WaitForSeconds(0.15f);
            }
            float fWaitTime = 0.26f + (0.15f * lNearbvLights.Count); // 0.26 is how long the flicker animation lasts, calculate how long we waited for all of the lights in loop above
            yield return new WaitForSeconds(fWaitTime);
            foreach (Animator light in lNearbvLights) light.SetBool("on", false);
        }

        [ServerRpc(RequireOwnership = false)]
        public void KillNearbyLightsServerRpc() {
            KillNearbyLightsClientRpc();
        }

        [ClientRpc]
        public void KillNearbyLightsClientRpc() {
            StartCoroutine(KillNearbyLightsCoroutine());
        }
    }
}
