(() => {
    "use strict";
    const raiz = document.querySelector("[data-localizacao]");
    if (!raiz) return;
    const formulario = raiz.closest("fieldset");
    const latitude = formulario.querySelector("#Latitude"), longitude = formulario.querySelector("#Longitude");
    const estado = raiz.querySelector("[data-estado]"), resultados = raiz.querySelector("[data-resultados]");
    const campoBusca = raiz.querySelector("#busca-municipio"), buscar = raiz.querySelector("[data-buscar]");
    raiz.querySelector("[data-controles-mapa]").hidden = false;
    let mapa, marcador, pedidoBusca, geracao = 0;
    const ler = campo => campo.value.trim() === "" ? NaN : Number(campo.value.trim().replace(",", "."));
    const pontoValido = (lat, lon) => Number.isFinite(lat) && Number.isFinite(lon) && Math.abs(lat) <= 90 && Math.abs(lon) <= 180;

    function sincronizar(centralizar = false) {
        const lat = ler(latitude), lon = ler(longitude);
        if (mapa && marcador) { mapa.removeLayer(marcador); marcador = null; }
        if (!pontoValido(lat, lon)) return;
        if (mapa) {
            marcador = L.circleMarker([lat, lon], { radius: 9, color: "#165c36", fillColor: "#42b66c", fillOpacity: 1, weight: 3 }).addTo(mapa);
            if (centralizar) mapa.setView([lat, lon], 14);
        }
    }
    function selecionar(lat, lon, mensagem) {
        if (!pontoValido(lat, lon)) { estado.textContent = "Não foi possível identificar um ponto válido."; return; }
        latitude.value = lat.toFixed(6).replace(".", ",");
        longitude.value = lon.toFixed(6).replace(".", ",");
        sincronizar(true);
        estado.textContent = mensagem + " Salve o rebanho para guardar a localização.";
    }
    try {
        if (!window.L) throw new Error("Mapa indisponível");
        const lat = ler(latitude), lon = ler(longitude), temPonto = pontoValido(lat, lon);
        mapa = L.map("mapa-localizacao", { center: temPonto ? [lat, lon] : [-27.81, -50.32], zoom: temPonto ? 14 : 7 });
        L.tileLayer("https://tile.openstreetmap.org/{z}/{x}/{y}.png", {
            maxZoom: 19, attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        }).on("tileerror", () => { estado.textContent = "O mapa está indisponível. Tente buscar o município, usar GPS ou informar as coordenadas."; }).addTo(mapa);
        mapa.on("click", evento => { geracao++; selecionar(evento.latlng.lat, evento.latlng.wrap().lng, "Ponto selecionado."); });
        sincronizar();
        estado.textContent = temPonto ? "Localização salva exibida no mapa." : "Nenhuma localização selecionada. Clique no mapa, busque um município ou use GPS.";
    } catch {
        estado.textContent = "O mapa está indisponível. Você pode buscar o município, usar GPS ou informar as coordenadas.";
        raiz.querySelector("#mapa-localizacao").hidden = true;
        raiz.querySelector("[data-centro]").disabled = true;
    }
    [latitude, longitude].forEach(campo => campo.addEventListener("input", () => { geracao++; sincronizar(); }));
    raiz.querySelector("[data-centro]").addEventListener("click", () => {
        if (!mapa) return;
        geracao++;
        const ponto = mapa.getCenter().wrap();
        selecionar(ponto.lat, ponto.lng, "Centro do mapa selecionado.");
    });
    raiz.querySelector("[data-limpar]").addEventListener("click", () => {
        geracao++; pedidoBusca?.abort();
        latitude.value = ""; longitude.value = ""; resultados.replaceChildren();
        sincronizar(); estado.textContent = "Localização removida do formulário. Salve para confirmar.";
    });
    raiz.querySelector("[data-gps]").addEventListener("click", () => {
        if (!navigator.geolocation) { estado.textContent = "GPS indisponível neste navegador. Use o mapa ou busque um município."; return; }
        const atual = ++geracao;
        estado.textContent = "Obtendo sua localização…";
        navigator.geolocation.getCurrentPosition(posicao => {
            if (atual !== geracao) return;
            selecionar(posicao.coords.latitude, posicao.coords.longitude, "Ponto do GPS selecionado. Confira se corresponde à propriedade.");
        }, () => {
            if (atual === geracao) estado.textContent = "Não foi possível acessar o GPS. Confira a permissão ou use o mapa e a busca de município.";
        }, { enableHighAccuracy: true, timeout: 10000, maximumAge: 0 });
    });
    async function buscarMunicipio() {
        const nome = campoBusca.value.trim();
        if (nome.length < 3) { estado.textContent = "Digite pelo menos três caracteres para buscar."; return; }
        const atual = ++geracao;
        pedidoBusca?.abort();
        const pedido = new AbortController(); pedidoBusca = pedido;
        const limite = setTimeout(() => pedido.abort(), 12000);
        buscar.disabled = true; resultados.replaceChildren(); estado.textContent = "Buscando municípios…";
        try {
            const resposta = await fetch(raiz.dataset.buscaUrl + "?nome=" + encodeURIComponent(nome), { signal: pedido.signal, headers: { Accept: "application/json" } });
            if (!resposta.ok || resposta.redirected) throw new Error("Busca indisponível");
            const itens = await resposta.json();
            if (atual !== geracao) return;
            if (!Array.isArray(itens)) throw new Error("Resposta inválida");
            estado.textContent = itens.length ? "Selecione o município e depois ajuste o ponto para a propriedade." : "Nenhum município encontrado. Tente acrescentar o estado.";
            itens.forEach(item => {
                const botao = document.createElement("button");
                botao.type = "button"; botao.className = "list-group-item list-group-item-action";
                botao.textContent = [item.nome, item.estado, item.pais].filter(Boolean).join(" — ");
                botao.addEventListener("click", () => { geracao++; selecionar(item.latitude, item.longitude, "Ponto de referência do município selecionado. Ajuste para a propriedade."); resultados.replaceChildren(); });
                resultados.append(botao);
            });
        } catch {
            if (atual === geracao) estado.textContent = "Busca indisponível. Tente novamente, use GPS ou selecione um ponto no mapa.";
        } finally { clearTimeout(limite); buscar.disabled = false; }
    }
    buscar.addEventListener("click", buscarMunicipio);
    campoBusca.addEventListener("keydown", evento => { if (evento.key === "Enter") { evento.preventDefault(); buscarMunicipio(); } });
})();
