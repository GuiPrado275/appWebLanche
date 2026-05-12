// Valida se as senhas coincidem no cadastro
function validarSenhas() {
    const senha = document.getElementById('senha');
    const confirmar = document.getElementById('confirmarSenha');
    const erro = document.getElementById('senhaErro');

    if (!senha || !confirmar) return true;

    if (senha.value !== confirmar.value) {
        if (erro) erro.style.display = 'block';
        confirmar.classList.add('is-invalid');
        return false;
    }

    if (erro) erro.style.display = 'none';
    confirmar.classList.remove('is-invalid');
    return true;
}

// Valida se as novas senhas coincidem na tela de alterar senha
function validarNovaSenha() {
    const nova = document.getElementById('novaSenha');
    const confirmar = document.getElementById('confirmarNovaSenha');
    const erro = document.getElementById('senhaErro');

    if (!nova || !confirmar) return true;

    if (nova.value !== confirmar.value) {
        if (erro) erro.style.display = 'block';
        confirmar.classList.add('is-invalid');
        return false;
    }

    if (erro) erro.style.display = 'none';
    confirmar.classList.remove('is-invalid');
    return true;
}

