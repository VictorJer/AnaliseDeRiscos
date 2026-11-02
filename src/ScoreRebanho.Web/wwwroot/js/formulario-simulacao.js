(() => {
    "use strict";

    document.querySelectorAll("[data-terreno-pastagem]").forEach(terreno => {
        const posse = terreno.querySelector("[data-posse]");
        const arrendamento = terreno.querySelector("[data-arrendamento]");
        if (!posse || !arrendamento) return;

        function atualizarVisibilidade() {
            const arrendado = posse.value === "2";
            arrendamento.hidden = !arrendado;
            arrendamento.querySelectorAll("input").forEach(campo => { campo.disabled = !arrendado; });
        }

        posse.addEventListener("change", atualizarVisibilidade);
        atualizarVisibilidade();
    });
})();
