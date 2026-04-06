<!--
  Star display: shows 1-3 filled/empty stars.
-->
<script lang="ts">
	let { count = 0, max = 3, size = '1.2rem' }: { count?: number; max?: number; size?: string } = $props();
</script>

<span class="stars" aria-label="{count} of {max} stars" style="font-size: {size};">
	{#each Array(max) as _, i}
		<span class="star" class:filled={i < count}>★</span>
	{/each}
</span>

<style>
	.stars {
		display: inline-flex;
		gap: 2px;
	}

	.star {
		color: var(--color-star-empty);
		transition: color var(--transition-fast);
	}

	.star.filled {
		color: var(--color-star-filled);
		text-shadow: 0 0 8px rgba(251, 191, 36, 0.5);
	}
</style>
