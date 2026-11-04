"""Falha o build se a cobertura de linhas de Domínio ou Aplicação ficar abaixo do mínimo."""
import sys
import xml.etree.ElementTree as ET

MINIMO = float(sys.argv[2]) if len(sys.argv) > 2 else 0.70
CAMADAS = ("Clyvo.PetCare.Domain", "Clyvo.PetCare.Application")

raiz = ET.parse(sys.argv[1]).getroot()
reprovado = False
for pacote in raiz.iter("package"):
    nome = pacote.get("name")
    if nome in CAMADAS:
        taxa = float(pacote.get("line-rate", 0))
        situacao = "OK" if taxa >= MINIMO else "ABAIXO DO MÍNIMO"
        print(f"{nome}: {taxa:.1%} de linhas cobertas (mínimo {MINIMO:.0%}) — {situacao}")
        reprovado |= taxa < MINIMO

sys.exit(1 if reprovado else 0)
