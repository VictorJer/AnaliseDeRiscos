(() => {
    "use strict";

    const chave = "score-rebanho.tema";
    const opcoes = ["light", "dark", "system"];
    const sistema = window.matchMedia("(prefers-color-scheme: dark)");
    let preferencia = lerPreferencia();

    function lerPreferencia() {
        try {
            const valor = window.localStorage.getItem(chave);
            return opcoes.includes(valor) ? valor : "system";
        } catch {
            return "system";
        }
    }

    function aplicarTema() {
        const tema = preferencia === "system" ? (sistema.matches ? "dark" : "light") : preferencia;
        document.documentElement.setAttribute("data-bs-theme", tema);
        const seletor = document.getElementById("tema");
        if (seletor) seletor.value = preferencia;
    }

    aplicarTema();
    sistema.addEventListener("change", () => {
        if (preferencia === "system") aplicarTema();
    });
    window.addEventListener("storage", (evento) => {
        if (evento.key === chave || evento.key === null) {
            preferencia = lerPreferencia();
            aplicarTema();
        }
    });

    document.addEventListener("DOMContentLoaded", () => {
        const seletor = document.getElementById("tema");
        const controle = document.getElementById("preferencia-tema");
        if (!seletor || !controle) return;
        aplicarTema();
        controle.hidden = false;
        seletor.addEventListener("change", () => {
            preferencia = opcoes.includes(seletor.value) ? seletor.value : "system";
            try {
                if (preferencia === "system") window.localStorage.removeItem(chave);
                else window.localStorage.setItem(chave, preferencia);
            } catch {
                // Sem armazenamento disponível, a escolha vale somente nesta página.
            }
            aplicarTema();
        });
    });
})();
