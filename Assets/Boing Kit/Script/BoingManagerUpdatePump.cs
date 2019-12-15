/******************************************************************************/
/*
  Project   - Boing Kit
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

using UnityEngine;

namespace BoingKit
{
  public class BoingManagerUpdatePump : MonoBehaviour
  {
    public void LateUpdate()
    {
      if (BoingManager.s_managerGo != gameObject)
      {
        // so reimporting scripts don't build up duplicate update pumps
        Destroy(gameObject);
        return;
      }

      BoingManager.LateUpdate();
    }
  }
}
