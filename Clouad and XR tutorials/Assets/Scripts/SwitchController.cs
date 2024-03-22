using UnityEngine;
// ȷ��������UnityEngine.XR.Interaction.Toolkit�����ռ䣬�����Ŀ��ؽ�������XR Interaction Toolkit
using UnityEngine.XR.Interaction.Toolkit;

public class SwitchController : MonoBehaviour
{
    // ͨ�������������õ�������SolderingIron�ű�
    public SolderingIron solderingIronScript;

    // һ���򵥵ķ����������л��������Ŀ���״̬
    public void TogglePower()
    {
        // ����Ƿ��Ѿ���ȷ�����˶Ե������ű�������
        if (solderingIronScript != null)
        {
            // �л��������Ŀ���״̬
            solderingIronScript.on = !solderingIronScript.on;

            // ���ݵ�ǰ�Ŀ���״̬���������һЩ�Ӿ�����Ƶ����
            // ���磬���ſ�����Ч��ı俪�ص���ɫ/���
        }
        else
        {
            Debug.LogWarning("SwitchController: SolderingIron script reference not set.");
        }
    }
}
