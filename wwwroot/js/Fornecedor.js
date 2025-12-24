document.addEventListener("DOMContentLoaded", carregarFornecedores);

function carregarFornecedores() {
    let fornecedores = JSON.parse(localStorage.getItem("fornecedores")) || [];
    atualizarTabela(fornecedores);
}

function atualizarTabela(fornecedores) {
    let tbody = document.getElementById("fornecedores");
    tbody.innerHTML = "";

    fornecedores.forEach(f => {
        let tr = document.createElement("tr");

        tr.innerHTML = `
            <td>${f.nome}</td>
            <td>${f.empresa}</td>
            <td>${f.telefone || "-"}</td>
            <td>${f.email || "-"}</td>
            <td>${f.produto || "-"}</td>
            <td>
                <select onchange="alterarStatus('${f.id}', this.value)">
                    <option value="Ativo" ${f.status === "Ativo" ? "selected" : ""}>Ativo</option>
                    <option value="Inativo" ${f.status === "Inativo" ? "selected" : ""}>Inativo</option>
                    <option value="Em negociação" ${f.status === "Em negociação" ? "selected" : ""}>Em negociação</option>
                </select>
            </td>
            <td>
                <button class="btn btn-edit" onclick="editarFornecedor('${f.id}')">✏️ Editar</button>
                <button class="btn btn-remove" onclick="removerFornecedor('${f.id}')">🗑️ Remover</button>
            </td>
        `;

        tbody.appendChild(tr);
    });
}

function mostrarFormulario() {
    document.getElementById("formulario").style.display = "block";
}

function cancelarFormulario() {
    document.getElementById("formulario").style.display = "none";
}

function salvarFornecedor() {
    let nome = document.getElementById("nome").value;
    let empresa = document.getElementById("empresa").value;
    let telefone = document.getElementById("telefone").value;
    let email = document.getElementById("email").value;
    let produto = document.getElementById("produto").value;

    if (!nome || !empresa) return;

    let fornecedor = {
        id: crypto.randomUUID(),
        nome: nome,
        empresa: empresa,
        telefone: telefone,
        email: email,
        produto: produto,
        status: "Ativo"
    };

    let fornecedores = JSON.parse(localStorage.getItem("fornecedores")) || [];
    fornecedores.push(fornecedor);
    localStorage.setItem("fornecedores", JSON.stringify(fornecedores));
    atualizarTabela(fornecedores);

    cancelarFormulario();
}

function editarFornecedor(id) {
    let fornecedores = JSON.parse(localStorage.getItem("fornecedores")) || [];
    let fornecedor = fornecedores.find(f => f.id === id);
    if (!fornecedor) return;

    fornecedor.nome = prompt("Nome:", fornecedor.nome) || fornecedor.nome;
    fornecedor.empresa = prompt("Empresa:", fornecedor.empresa) || fornecedor.empresa;
    fornecedor.telefone = prompt("Telefone:", fornecedor.telefone) || fornecedor.telefone;
    fornecedor.email = prompt("Email:", fornecedor.email) || fornecedor.email;
    fornecedor.produto = prompt("Produto/Serviço:", fornecedor.produto) || fornecedor.produto;

    localStorage.setItem("fornecedores", JSON.stringify(fornecedores));
    atualizarTabela(fornecedores);
}

function alterarStatus(id, novoStatus) {
    let fornecedores = JSON.parse(localStorage.getItem("fornecedores")) || [];
    let fornecedor = fornecedores.find(f => f.id === id);
    if (fornecedor) {
        fornecedor.status = novoStatus;
        localStorage.setItem("fornecedores", JSON.stringify(fornecedores));
    }
}

function removerFornecedor(id) {
    let fornecedores = JSON.parse(localStorage.getItem("fornecedores")) || [];
    fornecedores = fornecedores.filter(f => f.id !== id);
    localStorage.setItem("fornecedores", JSON.stringify(fornecedores));
    atualizarTabela(fornecedores);
}
