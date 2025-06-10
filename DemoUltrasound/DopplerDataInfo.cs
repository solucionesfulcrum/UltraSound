using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DemoUltrasound
{
    [StructLayout(LayoutKind.Sequential)]

    public struct D_PWDataInfo
    {
        public int m_nDateNum; // Número de datos
        public float m_AutoMeasureAngle; // Ángulo de medición automática
        public float m_BloodRadius_T; // Radio superior
        public float m_BloodRadius_B; // Radio inferior
        // Agrega otros campos según sea necesario
    }
}
