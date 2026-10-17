SELECT
    p.id
FROM public.perfis p
WHERE p.usuario_id = @UsuarioId
LIMIT 1;
