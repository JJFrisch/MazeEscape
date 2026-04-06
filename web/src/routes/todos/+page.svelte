<script lang="ts">
	import { onMount } from 'svelte';
	import { getSupabaseBrowserClient } from '$lib/supabase/client';

	type Todo = {
		id: string | number;
		name: string;
	};

	let todos = $state<Todo[]>([]);
	let loading = $state(true);
	let error = $state('');

	onMount(async () => {
		const supabase = getSupabaseBrowserClient();
		const { data, error: queryError } = await supabase.from('todos').select('id, name').order('id');

		if (queryError) {
			error = queryError.message;
		} else {
			todos = data ?? [];
		}

		loading = false;
	});
</script>

<svelte:head>
	<title>Todos - MazeEscape</title>
</svelte:head>

<section class="todos-page">
	<h1>Supabase Todos</h1>
	<p>This route is the SvelteKit equivalent of the Next.js example you provided.</p>

	{#if loading}
		<p>Loading todos...</p>
	{:else if error}
		<p class="error">{error}</p>
	{:else if todos.length === 0}
		<p>No todos found.</p>
	{:else}
		<ul>
			{#each todos as todo}
				<li>{todo.name}</li>
			{/each}
		</ul>
	{/if}
</section>

<style>
	.todos-page {
		max-width: 42rem;
		margin: 0 auto;
		padding: var(--space-4);
	}

	h1 {
		font-family: var(--font-display);
		font-size: var(--text-2xl);
		margin-bottom: var(--space-2);
	}

	p {
		color: var(--color-text-secondary);
		margin-bottom: var(--space-4);
	}

	ul {
		padding-left: 1.25rem;
	}

	li {
		margin-bottom: 0.5rem;
	}

	.error {
		color: var(--color-accent-red);
	}
</style>