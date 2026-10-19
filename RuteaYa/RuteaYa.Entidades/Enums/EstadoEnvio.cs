using System;
using System.Collections.Generic;
using System.Text;

public enum EstadoEnvio : byte
{
    BuscandoConductor = 1,
    ConductorAsignado = 2,
    Retirado = 3,
    EnCamino = 4,
    Entregado = 5,
    Cancelado = 6
}