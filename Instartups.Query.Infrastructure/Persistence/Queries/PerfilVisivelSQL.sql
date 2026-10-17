SELECT EXISTS (
    SELECT 1
    FROM public.perfis p
    WHERE p.id = @PerfilId
      AND (p.ativo OR p.id = @MeuPerfilId)
);
