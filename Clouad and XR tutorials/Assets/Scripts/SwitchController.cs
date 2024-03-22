using UnityEngine;
// 确保导入了UnityEngine.XR.Interaction.Toolkit命名空间，如果你的开关交互基于XR Interaction Toolkit
using UnityEngine.XR.Interaction.Toolkit;

public class SwitchController : MonoBehaviour
{
    // 通过公共变量引用电烙铁的SolderingIron脚本
    public SolderingIron solderingIronScript;

    // 一个简单的方法，用于切换电烙铁的开关状态
    public void TogglePower()
    {
        // 检查是否已经正确设置了对电烙铁脚本的引用
        if (solderingIronScript != null)
        {
            // 切换电烙铁的开关状态
            solderingIronScript.on = !solderingIronScript.on;

            // 根据当前的开关状态，可以添加一些视觉或音频反馈
            // 例如，播放开关音效或改变开关的颜色/外观
        }
        else
        {
            Debug.LogWarning("SwitchController: SolderingIron script reference not set.");
        }
    }
}
