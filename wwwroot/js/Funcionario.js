document.addEventListener("DOMContentLoaded", carregarFuncionarios);

function carregarFuncionarios() {
    let funcionarios = JSON.parse(localStorage.getItem("funcionarios")) || [];
    atualizarTabela(funcionarios);
}

function atualizarTabela(funcionarios) {
    let tbody = document.getElementById("funcionarios");
    tbody.innerHTML = "";

    funcionarios.forEach(f => {
        let tr = document.createElement("tr");

        tr.innerHTML = `
            <td>${f.nome}</td>
            <td>${f.cargo}</td>
            <td>${f.telefone || "-"}</td>
            <td>${f.email || "-"}</td>
            <td>
                <select onchange="alterarStatus('${f.id}', this.value)">
                    <option value="Ativo" ${f.status === "Ativo" ? "selected" : ""}>Ativo</option>
                    <option value="Folga" ${f.status === "Folga" ? "selected" : ""}>Folga</option>
                    <option value="Treinamento" ${f.status === "Treinamento" ? "selected" : ""}>Treinamento</option>
                    <option value="Férias" ${f.status === "Férias" ? "selected" : ""}>Férias</option>
                </select>
            </td>
            <td>
                <button class="btn btn-edit" onclick="editarFuncionario('${f.id}')">✏️ Editar</button>
                <button class="btn btn-remove" onclick="removerFuncionario('${f.id}')">🗑️ Remover</button>
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

function salvarFuncionario() {
    let nome = document.getElementById("nome").value;
    let cargo = document.getElementById("cargo").value;
    let telefone = document.getElementById("telefone").value;
    let email = document.getElementById("email").value;

    if (!nome || !cargo) return;

    let funcionario = {
        id: crypto.randomUUID(),
        nome: nome,
        cargo: cargo,
        telefone: telefone,
        email: email,
        status: "Ativo"
    };

    let funcionarios = JSON.parse(localStorage.getItem("funcionarios")) || [];
    funcionarios.push(funcionario);
    localStorage.setItem("funcionarios", JSON.stringify(funcionarios));
    atualizarTabela(funcionarios);

    cancelarFormulario();
}

function editarFuncionario(id) {
    let funcionarios = JSON.parse(localStorage.getItem("funcionarios")) || [];
    let funcionario = funcionarios.find(f => f.id === id);
    if (!funcionario) return;

    funcionario.nome = prompt("Nome:", funcionario.nome) || funcionario.nome;
    funcionario.cargo = prompt("Cargo:", funcionario.cargo) || funcionario.cargo;
    funcionario.telefone = prompt("Telefone:", funcionario.telefone) || funcionario.telefone;
    funcionario.email = prompt("Email:", funcionario.email) || funcionario.email;

    localStorage.setItem("funcionarios", JSON.stringify(funcionarios));
    atualizarTabela(funcionarios);
}

function alterarStatus(id, novoStatus) {
    let funcionarios = JSON.parse(localStorage.getItem("funcionarios")) || [];
    let funcionario = funcionarios.find(f => f.id === id);
    if (funcionario) {
        funcionario.status = novoStatus;
        localStorage.setItem("funcionarios", JSON.stringify(funcionarios));
    }
}

function removerFuncionario(id) {
    let funcionarios = JSON.parse(localStorage.getItem("funcionarios")) || [];
    funcionarios = funcionarios.filter(f => f.id !== id);
    localStorage.setItem("funcionarios", JSON.stringify(funcionarios));
    atualizarTabela(funcionarios);
}
