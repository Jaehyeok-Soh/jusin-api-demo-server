using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JusinChatServer.Model
{
    public class DTOPlayer
    {
        public uint m_iObjectId { get; set; }
        public uint m_iTargetId { get; set; }
        public int m_iHp { get; set; }
        public string m_strName { get; set; }
        public float fX { get; set; }
        public float fY { get; set; }
        public int m_iDir { get; set; }
        public string m_strFrameKey { get; set; }
        public int m_iFrameStart { get; set; }
        public int m_iState { get; set; }
        public int m_iBushOption { get; set; }
        public bool m_bIsUsingSkill { get; set; }
        public int m_iCurrentSkill { get; set; }
        public bool isStart { get; set; }
        public bool isQuit { get; set; }
        public string winner { get; set; } = string.Empty;
        public string accont { get; set; } = string.Empty;
        public bool isDead { get; set; } = false;
    }
}
