<!--
  Modal/popup component with backdrop.
-->
<script lang="ts">
	let {
		open = false,
		onclose,
		closable = true,
		children
	}: {
		open?: boolean;
		onclose?: () => void;
		closable?: boolean;
		children?: import('svelte').Snippet;
	} = $props();

	function handleBackdrop() {
		if (closable && onclose) onclose();
	}

	function handleKeydown(e: KeyboardEvent) {
		if (e.key === 'Escape' && closable && onclose) onclose();
	}
</script>

<svelte:window onkeydown={handleKeydown} />

{#if open}
	<div class="modal-backdrop" onclick={handleBackdrop} role="presentation">
		<div class="modal-content" onclick={(e) => e.stopPropagation()} role="dialog" aria-modal="true">
			{#if children}
				{@render children()}
			{/if}
		</div>
	</div>
{/if}

<style>
	.modal-backdrop {
		position: fixed;
		inset: 0;
		z-index: 200;
		display: flex;
		align-items: center;
		justify-content: center;
		background: var(--color-bg-overlay);
		backdrop-filter: blur(4px);
		animation: fadeIn 200ms ease;
		padding: var(--space-4);
	}

	.modal-content {
		background: var(--color-bg-card);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-xl);
		padding: var(--space-8);
		max-width: 480px;
		width: 100%;
		box-shadow: var(--shadow-lg);
		animation: scaleIn 200ms ease;
	}

	@keyframes fadeIn {
		from { opacity: 0; }
		to { opacity: 1; }
	}

	@keyframes scaleIn {
		from { transform: scale(0.95); opacity: 0; }
		to { transform: scale(1); opacity: 1; }
	}
</style>
